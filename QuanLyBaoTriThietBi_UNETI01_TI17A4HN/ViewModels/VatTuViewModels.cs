// Họ tên: Nguyễn Khắc Hiếu
// MSSV: 23103100225
// Nội dung: Module 5 - STT 57 & STT 58: ViewModel Quản lý danh mục Kho vật tư và Tích hợp xuất kho bảo toàn lịch sử
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public enum MucTonKhoCanhBao
{
    HetHang,     // SoLuongTon == 0
    SapHetHang,  // 0 < SoLuongTon <= 5
    AnToan       // SoLuongTon > 5
}

/// <summary>
/// Dòng hiển thị một vật tư trong danh sách kho
/// </summary>
public class VatTuItemViewModel
{
    public int MaVatTu { get; set; }

    [Display(Name = "Tên vật tư")]
    public string TenVatTu { get; set; } = string.Empty;

    [Display(Name = "Đơn vị tính")]
    public string DonViTinh { get; set; } = string.Empty;

    [Display(Name = "Đơn giá mặc định")]
    public decimal DonGiaMacDinh { get; set; }

    [Display(Name = "Số lượng tồn")]
    public int SoLuongTon { get; set; }

    [Display(Name = "Trạng thái")]
    public bool TrangThai { get; set; }

    [Display(Name = "Tổng giá trị tồn")]
    public decimal TongGiaTriTon => SoLuongTon * DonGiaMacDinh;

    [Display(Name = "Số lần xuất dùng")]
    public int SoLanSuDung { get; set; }

    /// <summary>
    /// Mức cảnh báo tồn kho (Hết hàng: 0, Sắp hết: <= 5, An toàn: > 5)
    /// </summary>
    public MucTonKhoCanhBao CanhBao => SoLuongTon <= 0 
        ? MucTonKhoCanhBao.HetHang 
        : (SoLuongTon <= 5 ? MucTonKhoCanhBao.SapHetHang : MucTonKhoCanhBao.AnToan);
}

/// <summary>
/// Form thêm mới / cập nhật danh mục vật tư
/// </summary>
public class VatTuFormViewModel
{
    public int MaVatTu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên vật tư / linh kiện")]
    [StringLength(150, ErrorMessage = "Tên vật tư tối đa 150 ký tự")]
    [Display(Name = "Tên vật tư")]
    public string TenVatTu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập đơn vị tính (Cái, Bộ, Bình, Mét...)")]
    [StringLength(30, ErrorMessage = "Đơn vị tính tối đa 30 ký tự")]
    [Display(Name = "Đơn vị tính")]
    public string DonViTinh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập đơn giá mặc định gợi ý")]
    [Range(0, 1_000_000_000, ErrorMessage = "Đơn giá phải từ 0 đến 1.000.000.000 ₫")]
    [Display(Name = "Đơn giá mặc định (₫)")]
    public decimal DonGiaMacDinh { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho ban đầu")]
    [Range(0, 100_000, ErrorMessage = "Số lượng tồn kho phải từ 0 đến 100.000")]
    [Display(Name = "Số lượng tồn kho")]
    public int SoLuongTon { get; set; }

    [Display(Name = "Trạng thái sử dụng")]
    public bool TrangThai { get; set; } = true;

    public bool LaThemMoi => MaVatTu == 0;
}

/// <summary>
/// Bộ lọc tìm kiếm danh mục kho vật tư
/// </summary>
public class VatTuFilterViewModel
{
    [Display(Name = "Tìm kiếm")]
    public string? TuKhoa { get; set; }

    [Display(Name = "Trạng thái")]
    public bool? TrangThai { get; set; }

    /// <summary>
    /// Lọc theo mức tồn: "TatCa", "SapHet" (<= 5), "HetHang" (== 0), "ConHang" (> 0)
    /// </summary>
    [Display(Name = "Mức tồn kho")]
    public string? MucTon { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Model hiển thị trang Index quản lý kho vật tư
/// </summary>
public class VatTuIndexViewModel
{
    public List<VatTuItemViewModel> Items { get; set; } = new();
    public VatTuFilterViewModel Filter { get; set; } = new();

    // Phân trang
    public int TotalItems { get; set; }
    public int PageIndex => Filter.Page;
    public int PageSize => Filter.PageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 10));

    // Thẻ số liệu thống kê nhanh ở đầu trang
    public int TongSoVatTu { get; set; }
    public int SoVatTuSapHet { get; set; }
    public int SoVatTuHetHang { get; set; }
    public decimal TongGiaTriTonKho { get; set; }

    // Phân quyền
    public bool LaAdmin { get; set; }
}
