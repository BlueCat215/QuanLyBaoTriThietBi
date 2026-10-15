// Họ tên: <điền họ tên>
// MSSV: <điền MSSV>
// Nội dung: Module 1 - Service Tài khoản (băm mật khẩu, không trùng tên đăng nhập, khóa/mở khóa, xóa an toàn)
#nullable enable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class TaiKhoanService : ITaiKhoanService
{
    // Cùng thuật toán với DbSeeder; AuthService (việc 12) dùng VerifyHashedPassword của chính lớp này.
    private static readonly PasswordHasher<TaiKhoan> _hasher = new();

    private readonly ITaiKhoanRepository _repo;

    public TaiKhoanService(ITaiKhoanRepository repo) => _repo = repo;

    // ---------------- Truy vấn ----------------
    public async Task<TaiKhoanIndexViewModel> LayDanhSachAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai, int? maNguoiThucHien)
        => new()
        {
            Items = await _repo.TimKiemAsync(tuKhoa, vaiTro, trangThai),
            TuKhoa = tuKhoa,
            VaiTro = vaiTro,
            TrangThai = trangThai,
            MaTaiKhoanHienTai = maNguoiThucHien
        };

    public async Task<TaiKhoanFormViewModel?> LayDeSuaAsync(int id)
    {
        var tk = await _repo.GetByIdAsync(id);
        if (tk is null) return null;

        return new TaiKhoanFormViewModel
        {
            MaTaiKhoan = tk.MaTaiKhoan,
            TenDangNhap = tk.TenDangNhap,
            HoTen = tk.HoTen,
            Email = tk.Email,
            VaiTro = tk.VaiTro,
            MaKhuVuc = tk.MaKhuVuc,
            TrangThai = tk.TrangThai
        };
    }

    public Task<List<KhuVucOption>> LayKhuVucChoChonAsync(int? giuLaiMa) => _repo.LayKhuVucChoChonAsync(giuLaiMa);

    // ---------------- Thêm ----------------
    public async Task<ServiceResult> ThemAsync(TaiKhoanFormViewModel vm)
    {
        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, hienTai: null);
        if (!loi.Succeeded) return loi;

        var tk = new TaiKhoan
        {
            TenDangNhap = vm.TenDangNhap,
            HoTen = vm.HoTen,
            Email = vm.Email,
            VaiTro = vm.VaiTro,
            MaKhuVuc = vm.MaKhuVuc,
            TrangThai = vm.TrangThai,
            NgayTaoTaiKhoan = DateTime.Now
        };
        tk.MatKhau = _hasher.HashPassword(tk, vm.MatKhau!);

        await _repo.AddAsync(tk);
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------- Sửa ----------------
    public async Task<ServiceResult> SuaAsync(TaiKhoanFormViewModel vm, int? maNguoiThucHien)
    {
        var tk = await _repo.GetByIdAsync(vm.MaTaiKhoan);
        if (tk is null)
            return ServiceResult.Fail("Không tìm thấy tài khoản.");

        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, tk);
        if (!loi.Succeeded) return loi;

        // Không tự khóa / tự đổi vai trò của chính mình
        if (maNguoiThucHien == tk.MaTaiKhoan && (!vm.TrangThai || vm.VaiTro != tk.VaiTro))
            return ServiceResult.Fail("Bạn không thể tự khóa hoặc tự đổi vai trò của chính tài khoản đang đăng nhập.");

        // Luôn còn ít nhất 1 quản trị viên hoạt động
        var matQuyenAdmin = tk.VaiTro == VaiTroEnum.Admin && tk.TrangThai
                            && !(vm.VaiTro == VaiTroEnum.Admin && vm.TrangThai);
        if (matQuyenAdmin && await _repo.DemAdminHoatDongAsync(boQuaId: tk.MaTaiKhoan) == 0)
            return ServiceResult.Fail("Hệ thống phải còn ít nhất một quản trị viên đang hoạt động.");

        tk.TenDangNhap = vm.TenDangNhap;
        tk.HoTen = vm.HoTen;
        tk.Email = vm.Email;
        tk.VaiTro = vm.VaiTro;
        tk.MaKhuVuc = vm.MaKhuVuc;

        if (!tk.TrangThai && vm.TrangThai)      // mở khóa qua form
            XoaTrangThaiKhoa(tk);
        tk.TrangThai = vm.TrangThai;

        if (!string.IsNullOrEmpty(vm.MatKhau))  // để trống = giữ mật khẩu cũ
            tk.MatKhau = _hasher.HashPassword(tk, vm.MatKhau);

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------- Khóa / Mở khóa ----------------
    public async Task<ServiceResult<bool>> DoiTrangThaiAsync(int id, int? maNguoiThucHien)
    {
        var tk = await _repo.GetByIdAsync(id);
        if (tk is null)
            return ServiceResult<bool>.Fail("Không tìm thấy tài khoản.");

        if (tk.TrangThai)   // đang hoạt động -> sắp khóa
        {
            if (maNguoiThucHien == id)
                return ServiceResult<bool>.Fail("Bạn không thể tự khóa tài khoản đang đăng nhập.");

            if (tk.VaiTro == VaiTroEnum.Admin && await _repo.DemAdminHoatDongAsync(boQuaId: id) == 0)
                return ServiceResult<bool>.Fail("Hệ thống phải còn ít nhất một quản trị viên đang hoạt động.");

            tk.TrangThai = false;
        }
        else                // đang khóa -> mở khóa, xóa luôn khóa tạm do đăng nhập sai
        {
            tk.TrangThai = true;
            XoaTrangThaiKhoa(tk);
        }

        await _repo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(tk.TrangThai);
    }

    // ---------------- Xóa an toàn ----------------
    public async Task<ServiceResult> XoaAsync(int id, int? maNguoiThucHien)
    {
        var tk = await _repo.GetByIdAsync(id);
        if (tk is null)
            return ServiceResult.Fail("Không tìm thấy tài khoản.");

        if (maNguoiThucHien == id)
            return ServiceResult.Fail("Bạn không thể xóa tài khoản đang đăng nhập.");

        if (tk.VaiTro == VaiTroEnum.Admin && tk.TrangThai && await _repo.DemAdminHoatDongAsync(boQuaId: id) == 0)
            return ServiceResult.Fail("Hệ thống phải còn ít nhất một quản trị viên đang hoạt động.");

        if (await _repo.CoDuLieuLienQuanAsync(id))
            return ServiceResult.Fail(
                $"Không thể xóa tài khoản \"{tk.TenDangNhap}\" vì đã phát sinh dữ liệu (báo cáo sự cố, nhật ký đăng nhập, thông báo, tệp đính kèm hoặc hồ sơ kỹ thuật viên). Hãy khóa tài khoản thay vì xóa.");

        try
        {
            _repo.Remove(tk);
            await _repo.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail("Không thể xóa: tài khoản còn dữ liệu liên quan. Hãy khóa tài khoản thay vì xóa.");
        }
        return ServiceResult.Ok();
    }

    // ---------------- Hàm nội bộ ----------------
    private static void XoaTrangThaiKhoa(TaiKhoan tk)
    {
        tk.SoLanDangNhapSai = 0;
        tk.KhoaDenNgay = null;
    }

    private static void ChuanHoa(TaiKhoanFormViewModel vm)
    {
        vm.TenDangNhap = (vm.TenDangNhap ?? string.Empty).Trim();
        vm.HoTen = (vm.HoTen ?? string.Empty).Trim();
        vm.Email = (vm.Email ?? string.Empty).Trim();

        // Admin / Kỹ thuật viên không bị giới hạn theo khu vực
        if (vm.VaiTro != VaiTroEnum.NguoiSuDung)
            vm.MaKhuVuc = null;
    }

    /// <param name="hienTai">null khi thêm mới; là tài khoản đang sửa khi cập nhật.</param>
    private async Task<ServiceResult> KiemTraAsync(TaiKhoanFormViewModel vm, TaiKhoan? hienTai)
    {
        var kq = ServiceResult.Ok();

        if (await _repo.TenDangNhapDaTonTaiAsync(vm.TenDangNhap, hienTai?.MaTaiKhoan))
            kq.AddError("Tên đăng nhập đã tồn tại", nameof(TaiKhoanFormViewModel.TenDangNhap));

        if (hienTai is null && string.IsNullOrEmpty(vm.MatKhau))
            kq.AddError("Vui lòng nhập mật khẩu", nameof(TaiKhoanFormViewModel.MatKhau));

        if (vm.VaiTro == VaiTroEnum.NguoiSuDung)
        {
            if (vm.MaKhuVuc is null)
                kq.AddError("Vui lòng chọn khu vực cho người sử dụng", nameof(TaiKhoanFormViewModel.MaKhuVuc));
            else if (vm.MaKhuVuc != hienTai?.MaKhuVuc && !await _repo.KhuVucDangHoatDongAsync(vm.MaKhuVuc.Value))
                kq.AddError("Khu vực không tồn tại hoặc đã ngừng hoạt động", nameof(TaiKhoanFormViewModel.MaKhuVuc));
        }

        return kq;
    }
}
