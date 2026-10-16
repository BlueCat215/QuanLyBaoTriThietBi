// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - ViewModel Kỹ thuật viên (danh sách, form Thêm/Sửa, kết quả đổi trạng thái)
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public record KyThuatVienRow(
    int MaKyThuatVien,
    string HoTen,
    string? ChuyenMon,
    string? SoDienThoai,
    string? Email,
    bool TrangThai,
    string? TenDangNhap,
    int SoPhanCongDangHieuLuc,
    int TongSoPhanCong
)
{
    /// Đã từng được phân công -> không xóa được, chỉ chuyển Ngừng hoạt động để giữ lịch sử.
    public bool CoLichSuPhanCong => TongSoPhanCong > 0;
}

public class KyThuatVienIndexViewModel
{
    public List<KyThuatVienRow> Items { get; set; } = new();
    public string? TuKhoa { get; set; }
    public bool? TrangThai { get; set; }
    public bool CoBoLoc => !string.IsNullOrWhiteSpace(TuKhoa) || TrangThai.HasValue;
}

/// Kết quả đổi trạng thái: HoatDong = trạng thái mới; SoPhanCongDangHieuLuc dùng để nhắc Admin đổi KTV nếu vừa ngừng.
public record KetQuaDoiTrangThaiKyThuatVien(bool HoatDong, int SoPhanCongDangHieuLuc);

public class KyThuatVienFormViewModel
{
    public int MaKyThuatVien { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Chuyên môn tối đa 150 ký tự")]
    [Display(Name = "Chuyên môn")]
    public string? ChuyenMon { get; set; }

    // Cho phép nhập có dấu cách/chấm/gạch (0912 345 678); Service chuẩn hóa lại và kiểm tra lần cuối.
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
    [RegularExpression(
        @"^(\+84|0)[0-9 .\-]{8,14}$",
        ErrorMessage = "Số điện thoại không hợp lệ (ví dụ: 0912345678 hoặc +84912345678)"
    )]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Tài khoản đăng nhập")]
    public int? MaTaiKhoan { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool TrangThai { get; set; } = true;

    public bool LaThemMoi => MaKyThuatVien == 0;

    [BindNever, ValidateNever]
    public List<SelectListItem> TaiKhoanOptions { get; set; } = new();
}
