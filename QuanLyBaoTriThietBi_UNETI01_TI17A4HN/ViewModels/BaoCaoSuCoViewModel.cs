// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - ViewModel Báo cáo sự cố (danh sách, chi tiết, form gửi/sửa)
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

/// Người đang thao tác (lấy từ Session/claim ở Controller). Admin xem được mọi báo cáo; người khác chỉ báo cáo của mình.
public record NguoiThaoTac(int MaTaiKhoan, bool LaAdmin);

/// Dữ liệu phát sinh từ 1 báo cáo (quyết định có xóa/hủy được không).
public record DuLieuLienQuanBaoCao(int SoPhieuXuLy, int SoPhieuConHieuLuc, int SoTepDinhKem)
{
    public bool CoDuLieu => SoPhieuXuLy + SoTepDinhKem > 0;
}

public record BaoCaoSuCoRow(
    int MaBaoCao,
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string NguoiBao,
    DateTime NgayBaoCao,
    string MoTaSuCo,
    MucDoSuCoEnum MucDoSuCo,
    TrangThaiBaoCaoEnum TrangThai
)
{
    public bool CoTheSua => TrangThai == TrangThaiBaoCaoEnum.MoiBao;
}

public class BaoCaoSuCoIndexViewModel
{
    public List<BaoCaoSuCoRow> Items { get; set; } = new();
    public string? TuKhoa { get; set; }
    public TrangThaiBaoCaoEnum? TrangThai { get; set; }
    public MucDoSuCoEnum? MucDo { get; set; }

    /// true = trang "Báo cáo của tôi" (người dùng); false = danh sách toàn bộ (Admin).
    public bool LaCuaToi { get; set; }

    public bool CoBoLoc =>
        !string.IsNullOrWhiteSpace(TuKhoa) || TrangThai.HasValue || MucDo.HasValue;
}

public record PhieuXuLyTomTat(
    int MaPhieuXuLy,
    LoaiXuLyEnum LoaiXuLy,
    TrangThaiPhieuXuLyEnum TrangThai,
    DateTime NgayTao
);

public class BaoCaoSuCoDetailsViewModel
{
    public required BaoCaoSuCo BaoCao { get; init; }
    public required DuLieuLienQuanBaoCao DuLieuLienQuan { get; init; }
    public List<PhieuXuLyTomTat> PhieuXuLys { get; init; } = new();
    public bool LaAdmin { get; init; }
    public bool CoTheSua { get; init; }
    public bool CoTheHuy { get; init; }
    public bool CoTheXoa { get; init; }
}

public class BaoCaoSuCoFormViewModel
{
    public int MaBaoCao { get; set; }

    // Khi sửa: thiết bị bị khóa, chỉ để hiển thị và gửi lại giá trị cũ
    [Required(ErrorMessage = "Vui lòng chọn thiết bị")]
    [Display(Name = "Thiết bị")]
    public int? MaThietBi { get; set; }

    [Required(ErrorMessage = "Vui lòng mô tả sự cố")]
    [StringLength(500, ErrorMessage = "Mô tả sự cố tối đa 500 ký tự")]
    [Display(Name = "Mô tả sự cố")]
    public string MoTaSuCo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn mức độ sự cố")]
    [Display(Name = "Mức độ sự cố")]
    public MucDoSuCoEnum? MucDoSuCo { get; set; } = MucDoSuCoEnum.TrungBinh;

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    // NgayBaoCao / TrangThai / người báo KHÔNG có trong form: do hệ thống tự gán, không cho nhập tay.

    public bool LaThemMoi => MaBaoCao == 0;

    [BindNever, ValidateNever]
    public List<SelectListItem> ThietBiOptions { get; set; } = new();
}
