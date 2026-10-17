// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - ViewModel Chi tiết xử lý (danh sách phiếu, chi tiết theo phiếu, form Thêm/Sửa)
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

/// Giới hạn nhập chung cho Annotation (form) và Service. Chọn sao cho SoLuong x DonGia luôn nằm trong decimal(18,2).
public static class ChiTietXuLyGioiHan
{
    public const int SoLuongToiDa = 100_000;
    public const string DonGiaToiDa = "1000000000"; // 1 tỷ đồng / đơn vị
}

/// Thông tin tóm tắt của một Phiếu xử lý, hiển thị đầu trang Chi tiết xử lý.
public record PhieuTomTat(
    int MaPhieuXuLy,
    LoaiXuLyEnum LoaiXuLy,
    TrangThaiPhieuXuLyEnum TrangThai,
    int? MaBaoCao,
    int? MaKeHoach,
    DateTime NgayTao,
    DateTime? NgayBatDau,
    DateTime? NgayHoanThanh,
    string? KetQuaXuLy,
    string? TenThietBi,
    string? SoSerial
)
{
    /// NƠI DUY NHẤT quy định khi nào được thêm/sửa/xóa chi tiết xử lý: chỉ khi phiếu đang xử lý.
    public bool ChoPhepGhiChiTiet => TrangThai == TrangThaiPhieuXuLyEnum.DangXuLy;

    /// "Sự cố SC-0003" / "Kế hoạch KH-0002".
    public string NguonText =>
        LoaiXuLy == LoaiXuLyEnum.XuLySuCo
            ? (MaBaoCao is { } bc ? $"Sự cố {UiHelper.Ma("SC", bc)}" : "Sự cố")
            : (MaKeHoach is { } kh ? $"Kế hoạch {UiHelper.Ma("KH", kh)}" : "Kế hoạch");
}

/// Một dòng ở danh sách chọn phiếu.
public record PhieuXuLyRow(
    int MaPhieuXuLy,
    LoaiXuLyEnum LoaiXuLy,
    TrangThaiPhieuXuLyEnum TrangThai,
    string? TenThietBi,
    string? SoSerial,
    DateTime NgayTao,
    int SoDong,
    decimal TongChiPhi
);

public class ChiTietXuLyPhieuListViewModel
{
    public List<PhieuXuLyRow> Items { get; set; } = new();
    public TrangThaiPhieuXuLyEnum? TrangThai { get; set; }
    public bool CoBoLoc => TrangThai.HasValue;
}

public record ChiTietXuLyRow(
    int MaChiTiet,
    DateTime NgayCapNhat,
    string NoiDungThucHien,
    string? VatTuLinhKien,
    int? SoLuong,
    decimal? DonGia,
    decimal? ThanhTien,
    string? GhiChu
);

public class ChiTietXuLyIndexViewModel
{
    public PhieuTomTat Phieu { get; set; } = null!;
    public List<ChiTietXuLyRow> Items { get; set; } = new();

    /// SUM(ThanhTien) - tính bằng truy vấn, không lưu cột.
    public decimal TongChiPhi { get; set; }
}

/// Vật tư đang dùng được, hiển thị trong ô chọn (đơn giá mặc định dùng để gợi ý).
public record VatTuLuaChon(int Ma, string Ten, string DonViTinh, decimal DonGia);

public class ChiTietXuLyFormViewModel
{
    public int MaChiTiet { get; set; }

    public int MaPhieuXuLy { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung thực hiện")]
    [StringLength(500, ErrorMessage = "Nội dung thực hiện tối đa 500 ký tự")]
    [Display(Name = "Nội dung thực hiện")]
    public string NoiDungThucHien { get; set; } = string.Empty;

    [Display(Name = "Vật tư trong kho")]
    public int? MaVatTu { get; set; }

    [StringLength(150, ErrorMessage = "Tên vật tư / linh kiện tối đa 150 ký tự")]
    [Display(Name = "Vật tư / linh kiện")]
    public string? VatTuLinhKien { get; set; }

    [Range(1, ChiTietXuLyGioiHan.SoLuongToiDa, ErrorMessage = "Số lượng phải > 0 (tối đa 100.000)")]
    [Display(Name = "Số lượng")]
    public int? SoLuong { get; set; }

    [Range(typeof(decimal), "0", ChiTietXuLyGioiHan.DonGiaToiDa, ErrorMessage = "Đơn giá phải >= 0 (tối đa 1.000.000.000)")]
    [Display(Name = "Đơn giá (₫)")]
    public decimal? DonGia { get; set; }

    [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public bool LaThemMoi => MaChiTiet == 0;

    // ---- Chỉ để hiển thị, không bind từ form ----
    [BindNever, ValidateNever]
    public PhieuTomTat? Phieu { get; set; }

    [BindNever, ValidateNever]
    public List<VatTuLuaChon> VatTuOptions { get; set; } = new();
}
