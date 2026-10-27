// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - ViewModel Kế hoạch bảo trì (danh sách, chi tiết vòng đời, form Thêm/Sửa)
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public record KeHoachRow(
    int MaKeHoach,
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string TenKhuVuc,
    DateTime NgayDuKien,
    string NoiDungBaoTri,
    MucDoUuTienEnum MucDoUuTien,
    TrangThaiKeHoachEnum TrangThai
)
{
    /// Kế hoạch còn "đang hoạt động" (chưa Hoàn thành / Đã hủy).
    public bool DangHoatDong => !KeHoachWorkflowHelper.IsLocked(TrangThai);

    public bool QuaHan => DangHoatDong && NgayDuKien.Date < DateTime.Today;

    public bool CoTheSua =>
        !KeHoachWorkflowHelper.IsLocked(TrangThai)
        && TrangThai is TrangThaiKeHoachEnum.ChoThucHien or TrangThaiKeHoachEnum.DaPhanCong;

    // Cho phép hủy khi chưa hoàn thành/đã hủy (Service sẽ kiểm tra thêm phiếu xử lý con)
    public bool CoTheHuy => KeHoachWorkflowHelper.CanTransitionTo(TrangThai, TrangThaiKeHoachEnum.DaHuy);

    public IReadOnlyList<TrangThaiKeHoachEnum> CacTrangThaiCoTheChuyen =>
        KeHoachWorkflowHelper.GetNextAllowedStatuses(TrangThai);
}

public class KeHoachBaoTriIndexViewModel
{
    public List<KeHoachRow> Items { get; set; } = new();
    public string? TuKhoa { get; set; }
    public TrangThaiKeHoachEnum? TrangThai { get; set; }
    public MucDoUuTienEnum? MucDo { get; set; }

    public bool CoBoLoc =>
        !string.IsNullOrWhiteSpace(TuKhoa) || TrangThai.HasValue || MucDo.HasValue;
}

public class KeHoachBaoTriFormViewModel
{
    public int MaKeHoach { get; set; }

    // Khi sửa: thiết bị bị khóa, chỉ hiển thị và gửi lại giá trị cũ
    [Required(ErrorMessage = "Vui lòng chọn thiết bị")]
    [Display(Name = "Thiết bị")]
    public int? MaThietBi { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày dự kiến")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Ngày dự kiến")]
    public DateTime? NgayDuKien { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bảo trì")]
    [StringLength(500, ErrorMessage = "Nội dung bảo trì tối đa 500 ký tự")]
    [Display(Name = "Nội dung bảo trì")]
    public string NoiDungBaoTri { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn mức độ ưu tiên")]
    [Display(Name = "Mức độ ưu tiên")]
    public MucDoUuTienEnum? MucDoUuTien { get; set; } = MucDoUuTienEnum.TrungBinh;

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    // Trạng thái KHÔNG có trong form: Thêm mới luôn là Chờ thực hiện, đổi trạng thái theo vòng đời (tuần 3).

    public bool LaThemMoi => MaKeHoach == 0;

    [BindNever, ValidateNever]
    public List<SelectListItem> ThietBiOptions { get; set; } = new();
}

public class KeHoachPhieuXuLyItem
{
    public int MaPhieuXuLy { get; set; }
    public LoaiXuLyEnum LoaiXuLy { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayHoanThanh { get; set; }
    public TrangThaiPhieuXuLyEnum TrangThai { get; set; }
    public string? KetQuaXuLy { get; set; }
    public string? KyThuatVienHienTai { get; set; }
}

public class KeHoachBaoTriChiTietViewModel
{
    public int MaKeHoach { get; set; }
    public int MaThietBi { get; set; }
    public string TenThietBi { get; set; } = string.Empty;
    public string SoSerial { get; set; } = string.Empty;
    public string TenLoaiThietBi { get; set; } = string.Empty;
    public string TenKhuVuc { get; set; } = string.Empty;
    public TinhTrangThietBiEnum TinhTrangThietBi { get; set; }
    public DateTime? NgayBaoTriGanNhat { get; set; }
    public DateTime NgayBaoTriTiepTheo { get; set; }

    public DateTime NgayDuKien { get; set; }
    public string NoiDungBaoTri { get; set; } = string.Empty;
    public MucDoUuTienEnum MucDoUuTien { get; set; }
    public TrangThaiKeHoachEnum TrangThai { get; set; }
    public string? GhiChu { get; set; }

    public bool DangHoatDong => !KeHoachWorkflowHelper.IsLocked(TrangThai);
    public bool QuaHan => DangHoatDong && NgayDuKien.Date < DateTime.Today;
    public bool CoTheSua =>
        !KeHoachWorkflowHelper.IsLocked(TrangThai)
        && TrangThai is TrangThaiKeHoachEnum.ChoThucHien or TrangThaiKeHoachEnum.DaPhanCong;
    public bool CoTheHuy => KeHoachWorkflowHelper.CanTransitionTo(TrangThai, TrangThaiKeHoachEnum.DaHuy);

    public IReadOnlyList<TrangThaiKeHoachEnum> CacTrangThaiCoTheChuyen =>
        KeHoachWorkflowHelper.GetNextAllowedStatuses(TrangThai);

    public List<KeHoachPhieuXuLyItem> DanhSachPhieuXuLy { get; set; } = new();
}
