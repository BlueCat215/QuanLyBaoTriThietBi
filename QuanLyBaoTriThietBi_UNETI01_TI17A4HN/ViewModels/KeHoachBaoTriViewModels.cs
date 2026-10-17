// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - ViewModel Kế hoạch bảo trì (danh sách, form Thêm/Sửa)
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public bool DangHoatDong =>
        TrangThai != TrangThaiKeHoachEnum.HoanThanh && TrangThai != TrangThaiKeHoachEnum.DaHuy;

    public bool QuaHan => DangHoatDong && NgayDuKien.Date < DateTime.Today;

    public bool CoTheSua =>
        TrangThai is TrangThaiKeHoachEnum.ChoThucHien or TrangThaiKeHoachEnum.DaPhanCong;

    // Service kiểm tra lại đầy đủ (còn phiếu xử lý đang hoạt động thì không hủy được)
    public bool CoTheHuy => TrangThai == TrangThaiKeHoachEnum.ChoThucHien;
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
