// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý Hệ thống & Danh mục dùng chung - IAuthService.cs
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

/// <summary>Thông tin lưu vào Session sau khi đăng nhập thành công.</summary>
public record PhienDangNhap(int MaTaiKhoan, string TenDangNhap, string HoTen, VaiTroEnum VaiTro);

public interface IAuthService
{
    /// <summary>
    /// Kiểm tra đăng nhập, ghi NhatKyDangNhap, cập nhật SoLanDangNhapSai/KhoaDenNgay/LanDangNhapCuoi.
    /// Thành công: Data = thông tin phiên. Thất bại: Errors chứa thông báo cho người dùng.
    /// </summary>
    Task<ServiceResult<PhienDangNhap>> DangNhapAsync(string? tenDangNhap, string? matKhau, string? diaChiIp);
}
