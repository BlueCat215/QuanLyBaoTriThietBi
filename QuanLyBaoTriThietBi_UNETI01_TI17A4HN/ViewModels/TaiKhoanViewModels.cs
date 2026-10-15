// Họ tên: <điền họ tên>
// MSSV: <điền MSSV>
// Nội dung: Module 1 - ViewModel Tài khoản (danh sách + form Thêm/Sửa, không lộ trường mật khẩu băm)
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public record KhuVucOption(int MaKhuVuc, string TenKhuVuc);

public record TaiKhoanRow(
    int MaTaiKhoan,
    string TenDangNhap,
    string HoTen,
    string Email,
    VaiTroEnum VaiTro,
    string? TenKhuVuc,
    bool TrangThai,
    DateTime? KhoaDenNgay,
    DateTime? LanDangNhapCuoi,
    bool CoDuLieuLienQuan);

public class TaiKhoanIndexViewModel
{
    public List<TaiKhoanRow> Items { get; set; } = new();
    public string? TuKhoa { get; set; }
    public VaiTroEnum? VaiTro { get; set; }
    public bool? TrangThai { get; set; }

    /// <summary>Tài khoản đang đăng nhập (null nếu chưa có đăng nhập) - dùng để ẩn nút Khóa/Xóa chính mình.</summary>
    public int? MaTaiKhoanHienTai { get; set; }

    public bool CoBoLoc => !string.IsNullOrWhiteSpace(TuKhoa) || VaiTro.HasValue || TrangThai.HasValue;
}

public class TaiKhoanFormViewModel
{
    public int MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự")]
    [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm, gạch dưới hoặc gạch ngang")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Vai trò")]
    public VaiTroEnum VaiTro { get; set; } = VaiTroEnum.NguoiSuDung;

    [Display(Name = "Khu vực")]
    public int? MaKhuVuc { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool TrangThai { get; set; } = true;

    // Thêm mới: bắt buộc (kiểm tra trong Service). Sửa: để trống = giữ nguyên mật khẩu cũ.
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
    [Display(Name = "Mật khẩu")]
    public string? MatKhau { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string? XacNhanMatKhau { get; set; }

    public bool LaThemMoi => MaTaiKhoan == 0;

    [BindNever, ValidateNever]
    public List<SelectListItem> KhuVucOptions { get; set; } = new();
}
