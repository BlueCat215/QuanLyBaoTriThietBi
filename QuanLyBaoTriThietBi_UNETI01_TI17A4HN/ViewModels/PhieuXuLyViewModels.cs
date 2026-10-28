// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - ViewModel Phiếu xử lý và Phân công xử lý
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

/// <summary>
/// Bộ lọc danh sách phiếu xử lý
/// </summary>
public class PhieuXuLyBoLoc
{
    public string? TuKhoa { get; set; }
    public LoaiXuLyEnum? LoaiXuLy { get; set; }
    public TrangThaiPhieuXuLyEnum? TrangThai { get; set; }
    public int? MaKyThuatVien { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public bool CoBoLoc =>
        !string.IsNullOrWhiteSpace(TuKhoa)
        || LoaiXuLy.HasValue
        || TrangThai.HasValue
        || MaKyThuatVien.HasValue
        || TuNgay.HasValue
        || DenNgay.HasValue;

    public bool KhoangNgayKhongHopLe => TuNgay.HasValue && DenNgay.HasValue && TuNgay.Value > DenNgay.Value;
}

/// <summary>
/// Dòng hiển thị trong bảng danh sách phiếu xử lý
/// </summary>
public record PhieuXuLyDanhSachRow(
    int MaPhieuXuLy,
    LoaiXuLyEnum LoaiXuLy,
    TrangThaiPhieuXuLyEnum TrangThai,
    int? MaBaoCao,
    int? MaKeHoach,
    string TenThietBi,
    string SoSerial,
    string? TenKhuVuc,
    int? MaKtvHienTai,
    string? TenKtvHienTai,
    DateTime NgayTao,
    DateTime? NgayBatDau,
    DateTime? NgayHoanThanh,
    decimal TongChiPhi
)
{
    public string NguonText =>
        LoaiXuLy == LoaiXuLyEnum.XuLySuCo
            ? (MaBaoCao.HasValue ? $"Sự cố {UiHelper.Ma("SC", MaBaoCao.Value)}" : "Sự cố")
            : (MaKeHoach.HasValue ? $"Kế hoạch {UiHelper.Ma("KH", MaKeHoach.Value)}" : "Kế hoạch");

    public bool CoTheBatDau => TrangThai == TrangThaiPhieuXuLyEnum.DaPhanCong;
}

/// <summary>
/// ViewModel màn hình Index Phiếu xử lý
/// </summary>
public class PhieuXuLyIndexViewModel
{
    public PagedResult<PhieuXuLyDanhSachRow> PagedItems { get; set; } = new();
    public PhieuXuLyBoLoc BoLoc { get; set; } = new();
    public List<SelectListItem> KtvOptions { get; set; } = new();
}

/// <summary>
/// Dòng lịch sử phân công của 1 phiếu
/// </summary>
public record PhanCongXuLyLichSuRow(
    int MaPhanCong,
    int MaKyThuatVien,
    string TenKyThuatVien,
    string? ChuyenMon,
    string? SoDienThoai,
    string? Email,
    DateTime NgayPhanCong,
    DateTime? NgayKetThucPhanCong,
    string? NoiDungPhanCong,
    TrangThaiPhanCongEnum TrangThai
);

/// <summary>
/// Chi tiết phiếu xử lý kèm lịch sử phân công và thông tin nguồn
/// </summary>
public class PhieuXuLyDetailsViewModel
{
    public int MaPhieuXuLy { get; set; }
    public LoaiXuLyEnum LoaiXuLy { get; set; }
    public TrangThaiPhieuXuLyEnum TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayHoanThanh { get; set; }
    public string? KetQuaXuLy { get; set; }
    public decimal TongChiPhi { get; set; }

    // Thông tin thiết bị
    public int MaThietBi { get; set; }
    public string TenThietBi { get; set; } = string.Empty;
    public string SoSerial { get; set; } = string.Empty;
    public string? TenLoaiThietBi { get; set; }
    public string? TenKhuVuc { get; set; }
    public TinhTrangThietBiEnum TinhTrangThietBi { get; set; }

    // Nguồn
    public int? MaBaoCao { get; set; }
    public string? MoTaSuCo { get; set; }
    public MucDoSuCoEnum? MucDoSuCo { get; set; }
    public string? NguoiBaoCao { get; set; }

    public int? MaKeHoach { get; set; }
    public string? NoiDungBaoTri { get; set; }
    public MucDoUuTienEnum? MucDoUuTien { get; set; }
    public DateTime? NgayDuKienBaoTri { get; set; }

    // KTV hiện tại
    public PhanCongXuLyLichSuRow? KtvHienTai { get; set; }

    // Lịch sử phân công
    public List<PhanCongXuLyLichSuRow> LichSuPhanCong { get; set; } = new();

    // Quyền thao tác
    public bool LaAdmin { get; set; }
    public bool LaKtvPhuTrach { get; set; }
    public bool CoThePhanCong => LaAdmin && (TrangThai == TrangThaiPhieuXuLyEnum.ChoPhanCong || KtvHienTai == null);
    public bool CoTheDoiKtv => LaAdmin && KtvHienTai != null && TrangThai != TrangThaiPhieuXuLyEnum.HoanThanh && TrangThai != TrangThaiPhieuXuLyEnum.DaHuy;
    public bool CoTheHuy => LaAdmin && TrangThai != TrangThaiPhieuXuLyEnum.HoanThanh && TrangThai != TrangThaiPhieuXuLyEnum.DaHuy;
    public bool CoTheBatDau => (TrangThai == TrangThaiPhieuXuLyEnum.DaPhanCong || (TrangThai == TrangThaiPhieuXuLyEnum.ChoPhanCong && KtvHienTai != null)) && (LaAdmin || LaKtvPhuTrach);

    // Dropdown KTV khi phân công / đổi KTV
    public List<SelectListItem> KtvOptions { get; set; } = new();
}

/// <summary>
/// Form tạo phiếu xử lý mới (thủ công hoặc từ nguồn)
/// </summary>
public class TaoPhieuXuLyViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn loại xử lý")]
    [Display(Name = "Loại xử lý")]
    public LoaiXuLyEnum LoaiXuLy { get; set; } = LoaiXuLyEnum.XuLySuCo;

    [Display(Name = "Báo cáo sự cố")]
    public int? MaBaoCao { get; set; }

    [Display(Name = "Kế hoạch bảo trì")]
    public int? MaKeHoach { get; set; }

    [Display(Name = "Kỹ thuật viên phụ trách (không bắt buộc)")]
    public int? MaKyThuatVien { get; set; }

    [StringLength(500, ErrorMessage = "Nội dung phân công tối đa 500 ký tự")]
    [Display(Name = "Nội dung / Chỉ đạo phân công")]
    public string? NoiDungPhanCong { get; set; }

    [Display(Name = "Chuyển thiết bị sang Tạm ngừng do sự cố (đối với sự cố Cao / Khẩn cấp)")]
    public bool TamNgungThietBi { get; set; }

    [BindNever, ValidateNever]
    public List<SelectListItem> BaoCaoOptions { get; set; } = new();

    [BindNever, ValidateNever]
    public List<SelectListItem> KeHoachOptions { get; set; } = new();

    [BindNever, ValidateNever]
    public List<SelectListItem> KtvOptions { get; set; } = new();
}

/// <summary>
/// Form phân công xử lý lần đầu
/// </summary>
public class PhanCongXuLyFormViewModel
{
    public int MaPhieuXuLy { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn kỹ thuật viên")]
    [Display(Name = "Kỹ thuật viên")]
    public int? MaKyThuatVien { get; set; }

    [StringLength(500, ErrorMessage = "Nội dung phân công tối đa 500 ký tự")]
    [Display(Name = "Nội dung phân công / Yêu cầu công việc")]
    public string? NoiDungPhanCong { get; set; }

    [BindNever, ValidateNever]
    public List<SelectListItem> KtvOptions { get; set; } = new();
}

/// <summary>
/// Form đổi kỹ thuật viên (STT 36)
/// </summary>
public class DoiKyThuatVienFormViewModel
{
    public int MaPhieuXuLy { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn kỹ thuật viên mới")]
    [Display(Name = "Kỹ thuật viên mới")]
    public int? MaKyThuatVienMoi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do đổi kỹ thuật viên để lưu lịch sử")]
    [StringLength(500, ErrorMessage = "Lý do tối đa 500 ký tự")]
    [Display(Name = "Lý do đổi kỹ thuật viên / Bàn giao")]
    public string LyDoDoi { get; set; } = string.Empty;

    [BindNever, ValidateNever]
    public List<SelectListItem> KtvOptions { get; set; } = new();
}
