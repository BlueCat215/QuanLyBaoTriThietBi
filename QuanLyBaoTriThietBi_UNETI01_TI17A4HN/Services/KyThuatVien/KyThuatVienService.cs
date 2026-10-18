// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Service Kỹ thuật viên (validate email/SĐT, liên kết tài khoản, ngừng hoạt động, xóa an toàn)
#nullable enable
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class KyThuatVienService : IKyThuatVienService
{
    // Sau khi bỏ dấu cách/chấm/gạch: 0 hoặc +84 rồi 9-10 chữ số
    private static readonly Regex MauSoDienThoai = new(@"^(\+84|0)\d{9,10}$", RegexOptions.Compiled);

    private readonly IKyThuatVienRepository _repo;

    public KyThuatVienService(IKyThuatVienRepository repo) => _repo = repo;

    // ---------------- Truy vấn ----------------
    public async Task<KyThuatVienIndexViewModel> LayDanhSachAsync(string? tuKhoa, bool? trangThai) =>
        new()
        {
            TuKhoa = tuKhoa?.Trim(),
            TrangThai = trangThai,
            Items = await _repo.TimKiemAsync(tuKhoa, trangThai),
        };

    public async Task<KyThuatVienFormViewModel?> LayDeSuaAsync(int id)
    {
        var k = await _repo.GetByIdAsync(id);
        if (k is null)
            return null;

        return new KyThuatVienFormViewModel
        {
            MaKyThuatVien = k.MaKyThuatVien,
            HoTen = k.HoTen,
            ChuyenMon = k.ChuyenMon,
            SoDienThoai = k.SoDienThoai,
            Email = k.Email,
            MaTaiKhoan = k.MaTaiKhoan,
            TrangThai = k.TrangThai,
        };
    }

    public Task<List<(int Ma, string Ten)>> LayTaiKhoanChoChonAsync(int? giuLaiMa) =>
        _repo.LayTaiKhoanChoChonAsync(giuLaiMa);

    public Task<List<(int Ma, string Ten)>> LayDangHoatDongChoChonAsync() =>
        _repo.LayDangHoatDongAsync();

    // ---------------- Thêm ----------------
    public async Task<ServiceResult> ThemAsync(KyThuatVienFormViewModel vm)
    {
        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, hienTai: null);
        if (!loi.Succeeded)
            return loi;

        await _repo.AddAsync(
            new KyThuatVien
            {
                HoTen = vm.HoTen,
                ChuyenMon = vm.ChuyenMon,
                SoDienThoai = vm.SoDienThoai,
                Email = vm.Email,
                MaTaiKhoan = vm.MaTaiKhoan,
                TrangThai = vm.TrangThai,
            }
        );
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------- Sửa ----------------
    public async Task<ServiceResult> SuaAsync(KyThuatVienFormViewModel vm)
    {
        var k = await _repo.GetByIdAsync(vm.MaKyThuatVien);
        if (k is null)
            return ServiceResult.Fail("Không tìm thấy kỹ thuật viên.");

        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, k);
        if (!loi.Succeeded)
            return loi;

        k.HoTen = vm.HoTen;
        k.ChuyenMon = vm.ChuyenMon;
        k.SoDienThoai = vm.SoDienThoai;
        k.Email = vm.Email;
        k.MaTaiKhoan = vm.MaTaiKhoan;
        k.TrangThai = vm.TrangThai;

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------- Đổi trạng thái ----------------
    public async Task<ServiceResult<KetQuaDoiTrangThaiKyThuatVien>> DoiTrangThaiAsync(int id)
    {
        var k = await _repo.GetByIdAsync(id);
        if (k is null)
            return ServiceResult<KetQuaDoiTrangThaiKyThuatVien>.Fail("Không tìm thấy kỹ thuật viên.");

        k.TrangThai = !k.TrangThai;
        await _repo.SaveChangesAsync();

        // Ngừng hoạt động không hủy phân công cũ (giữ lịch sử) nên báo số phân công còn hiệu lực để Admin đổi KTV
        var conHieuLuc = k.TrangThai ? 0 : await _repo.DemPhanCongAsync(id, chiDangHieuLuc: true);
        return ServiceResult<KetQuaDoiTrangThaiKyThuatVien>.Ok(
            new KetQuaDoiTrangThaiKyThuatVien(k.TrangThai, conHieuLuc)
        );
    }

    // ---------------- Xóa an toàn ----------------
    public async Task<ServiceResult> XoaAsync(int id)
    {
        var k = await _repo.GetByIdAsync(id);
        if (k is null)
            return ServiceResult.Fail("Không tìm thấy kỹ thuật viên.");

        var soPhanCong = await _repo.DemPhanCongAsync(id, chiDangHieuLuc: false);
        if (soPhanCong > 0)
            return ServiceResult.Fail(
                $"Không thể xóa kỹ thuật viên \"{k.HoTen}\" vì đã có {soPhanCong} phân công xử lý. "
                    + "Hãy chuyển sang Ngừng hoạt động để giữ lịch sử."
            );

        try
        {
            _repo.Remove(k);
            await _repo.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail(
                "Không thể xóa: kỹ thuật viên còn dữ liệu liên quan. Hãy chuyển sang Ngừng hoạt động."
            );
        }
        return ServiceResult.Ok();
    }

    // ---------------- Dùng cho Phân công (việc 35) ----------------
    public async Task<ServiceResult> KiemTraNhanPhanCongAsync(int maKyThuatVien)
    {
        var k = await _repo.GetByIdAsync(maKyThuatVien);
        if (k is null)
            return ServiceResult.Fail("Kỹ thuật viên không tồn tại.", "MaKyThuatVien");
        if (!k.TrangThai)
            return ServiceResult.Fail(
                $"Kỹ thuật viên \"{k.HoTen}\" đã ngừng hoạt động, không thể nhận phân công mới.",
                "MaKyThuatVien"
            );
        return ServiceResult.Ok();
    }

    // ---------------- Hàm nội bộ ----------------
    private static void ChuanHoa(KyThuatVienFormViewModel vm)
    {
        vm.HoTen = (vm.HoTen ?? string.Empty).Trim();
        vm.ChuyenMon = Rong(vm.ChuyenMon);
        vm.Email = Rong(vm.Email)?.ToLowerInvariant();

        var sdt = Rong(vm.SoDienThoai);
        vm.SoDienThoai = sdt is null ? null : Regex.Replace(sdt, @"[\s.\-]", "");
    }

    private static string? Rong(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <param name="hienTai">null khi thêm mới; là KTV đang sửa khi cập nhật.</param>
    private async Task<ServiceResult> KiemTraAsync(KyThuatVienFormViewModel vm, KyThuatVien? hienTai)
    {
        var kq = ServiceResult.Ok();

        if (vm.HoTen.Length == 0)
            kq.AddError("Vui lòng nhập họ tên", nameof(KyThuatVienFormViewModel.HoTen));

        // SĐT hợp lệ (tùy chọn)
        if (vm.SoDienThoai is not null && !MauSoDienThoai.IsMatch(vm.SoDienThoai))
            kq.AddError(
                "Số điện thoại không hợp lệ (ví dụ: 0912345678 hoặc +84912345678)",
                nameof(KyThuatVienFormViewModel.SoDienThoai)
            );

        // Email không trùng KTV khác (tùy chọn)
        if (vm.Email is not null && await _repo.EmailDaTonTaiAsync(vm.Email, hienTai?.MaKyThuatVien))
            kq.AddError(
                "Email đã được dùng cho kỹ thuật viên khác",
                nameof(KyThuatVienFormViewModel.Email)
            );

        // Tài khoản liên kết (tùy chọn): giữ nguyên nếu không đổi
        if (vm.MaTaiKhoan is { } maTk && maTk != hienTai?.MaTaiKhoan)
        {
            var tk = await _repo.LayTaiKhoanAsync(maTk);
            if (tk is null)
                kq.AddError("Tài khoản không tồn tại", nameof(KyThuatVienFormViewModel.MaTaiKhoan));
            else if (tk.VaiTro != VaiTroEnum.KyThuatVien)
                kq.AddError(
                    "Chỉ liên kết được tài khoản có vai trò Kỹ thuật viên",
                    nameof(KyThuatVienFormViewModel.MaTaiKhoan)
                );
            else if (!tk.TrangThai)
                kq.AddError("Tài khoản đang bị khóa", nameof(KyThuatVienFormViewModel.MaTaiKhoan));
            else if (await _repo.TaiKhoanDaGanHoSoKhacAsync(maTk, hienTai?.MaKyThuatVien))
                kq.AddError(
                    "Tài khoản này đã được gắn cho kỹ thuật viên khác",
                    nameof(KyThuatVienFormViewModel.MaTaiKhoan)
                );
        }

        return kq;
    }
}
