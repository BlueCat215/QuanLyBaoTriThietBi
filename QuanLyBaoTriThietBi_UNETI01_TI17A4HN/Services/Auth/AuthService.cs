// Họ tên: Nguyễn Văn Hoàng
// MSSV: 231030100303
// Nội dung: Module 1 - Service đăng nhập (EF Core + LINQ): kiểm tra mật khẩu băm, chặn tài khoản khóa, khóa tạm sau nhiều lần sai, ghi nhật ký
#nullable enable
using Microsoft.AspNetCore.Identity;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class AuthService : IAuthService
{
    private const int SoLanSaiToiDa = 5;                 // theo THIETKE.md: ví dụ 5 lần
    private const int PhutKhoaTam = 15;                  // ... khóa tạm 15 phút
    private const string ThongBaoSai = "Tên đăng nhập hoặc mật khẩu không đúng.";

    // Cùng thuật toán băm với DbSeeder và TaiKhoanService
    private static readonly PasswordHasher<TaiKhoan> _hasher = new();

    private readonly IRepository<TaiKhoan> _taiKhoan;
    private readonly IRepository<NhatKyDangNhap> _nhatKy;

    public AuthService(IRepository<TaiKhoan> taiKhoan, IRepository<NhatKyDangNhap> nhatKy)
    {
        _taiKhoan = taiKhoan;
        _nhatKy = nhatKy;
    }

    public async Task<ServiceResult<PhienDangNhap>> DangNhapAsync(string? tenDangNhap, string? matKhau, string? diaChiIp)
    {
        var ten = (tenDangNhap ?? string.Empty).Trim();
        if (ten.Length == 0 || string.IsNullOrEmpty(matKhau))
            return ServiceResult<PhienDangNhap>.Fail("Vui lòng nhập tên đăng nhập và mật khẩu.");

        var bayGio = DateTime.Now;
        var tk = await _taiKhoan.FirstOrDefaultAsync(t => t.TenDangNhap == ten);

        // 1. Không tồn tại
        if (tk is null)
        {
            await GhiNhatKyAsync(null, ten, KetQuaDangNhapEnum.KhongTonTaiTaiKhoan, diaChiIp, bayGio);
            return ServiceResult<PhienDangNhap>.Fail(ThongBaoSai);
        }

        // 2. Bị Admin khóa
        if (!tk.TrangThai)
        {
            await GhiNhatKyAsync(tk.MaTaiKhoan, ten, KetQuaDangNhapEnum.TaiKhoanBiKhoa, diaChiIp, bayGio);
            return ServiceResult<PhienDangNhap>.Fail("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
        }

        // 3. Đang bị khóa tạm do nhập sai nhiều lần (không kiểm tra mật khẩu để tránh dò mật khẩu)
        if (tk.KhoaDenNgay is { } khoaDen && khoaDen > bayGio)
        {
            var phutConLai = (int)Math.Ceiling((khoaDen - bayGio).TotalMinutes);
            await GhiNhatKyAsync(tk.MaTaiKhoan, ten, KetQuaDangNhapEnum.TaiKhoanBiKhoa, diaChiIp, bayGio);
            return ServiceResult<PhienDangNhap>.Fail(
                $"Tài khoản đang bị tạm khóa do nhập sai mật khẩu nhiều lần. Vui lòng thử lại sau {phutConLai} phút.");
        }

        // 4. Kiểm tra mật khẩu
        var ketQua = _hasher.VerifyHashedPassword(tk, tk.MatKhau, matKhau);
        if (ketQua == PasswordVerificationResult.Failed)
        {
            tk.SoLanDangNhapSai++;
            var vuaBiKhoa = tk.SoLanDangNhapSai >= SoLanSaiToiDa;
            if (vuaBiKhoa)
            {
                tk.KhoaDenNgay = bayGio.AddMinutes(PhutKhoaTam);
                tk.SoLanDangNhapSai = 0;   // đếm lại từ đầu sau khi hết thời gian khóa
            }

            await GhiNhatKyAsync(tk.MaTaiKhoan, ten, KetQuaDangNhapEnum.SaiMatKhau, diaChiIp, bayGio); // đồng thời lưu tk

            return ServiceResult<PhienDangNhap>.Fail(vuaBiKhoa
                ? $"Bạn đã nhập sai mật khẩu {SoLanSaiToiDa} lần. Tài khoản bị tạm khóa {PhutKhoaTam} phút."
                : ThongBaoSai);
        }

        // 5. Thành công
        if (ketQua == PasswordVerificationResult.SuccessRehashNeeded)
            tk.MatKhau = _hasher.HashPassword(tk, matKhau);

        tk.SoLanDangNhapSai = 0;
        tk.KhoaDenNgay = null;
        tk.LanDangNhapCuoi = bayGio;

        await GhiNhatKyAsync(tk.MaTaiKhoan, ten, KetQuaDangNhapEnum.ThanhCong, diaChiIp, bayGio); // đồng thời lưu tk

        return ServiceResult<PhienDangNhap>.Ok(new PhienDangNhap(tk.MaTaiKhoan, tk.TenDangNhap, tk.HoTen, tk.VaiTro));
    }

    // Mọi lần thử đều được ghi (kể cả sai tên đăng nhập -> MaTaiKhoan = null).
    // Các Repository dùng chung 1 DbContext nên SaveChanges ở đây lưu luôn thay đổi của TaiKhoan.
    private async Task GhiNhatKyAsync(int? maTaiKhoan, string tenNhap, KetQuaDangNhapEnum ketQua, string? ip, DateTime thoiGian)
    {
        await _nhatKy.AddAsync(new NhatKyDangNhap
        {
            MaTaiKhoan = maTaiKhoan,
            TenDangNhapNhap = tenNhap.Length > 50 ? tenNhap[..50] : tenNhap,
            ThoiGian = thoiGian,
            KetQua = ketQua,
            DiaChiIP = ip is { Length: > 45 } ? ip[..45] : ip
        });
        await _nhatKy.SaveChangesAsync();
    }
}
