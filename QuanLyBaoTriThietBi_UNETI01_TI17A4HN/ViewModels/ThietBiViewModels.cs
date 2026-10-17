// ViewModel Thiết bị (danh sách, chi tiết, form Thêm/Sửa)
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

///Số bản ghi của các module khác đang tham chiếu tới 1 thiết bị (dùng quyết định có xóa được không).
public record DuLieuLienQuanThietBi(int SoKeHoach, int SoBaoCao, int SoThongBao, int SoTepDinhKem)
{
    public bool CoDuLieu => SoKeHoach + SoBaoCao + SoThongBao + SoTepDinhKem > 0;
}

public record ThietBiRow(
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string TenLoaiThietBi,
    string TenKhuVuc,
    TinhTrangThietBiEnum TinhTrang,
    DateTime NgayBaoTriTiepTheo
);

public class ThietBiIndexViewModel
{
    public List<ThietBiRow> Items { get; set; } = new();

    ///Điều kiện Tìm kiếm + Lọc + Sắp xếp hiện tại (đổ lại vào form để giữ nguyên điều kiện).
    public ThietBiBoLoc BoLoc { get; set; } = new();

    public List<(int Ma, string Ten)> LoaiDeLoc { get; set; } = new();
    public List<(int Ma, string Ten)> KhuVucDeLoc { get; set; } = new();

    ///Thông báo khi có điều kiện lọc không hợp lệ (đã bị bỏ qua).
    public string? CanhBao { get; set; }

    public bool CoBoLoc => BoLoc.CoBoLoc;
}

public class ThietBiDetailsViewModel
{
    public required ThietBi ThietBi { get; init; }
    public required DuLieuLienQuanThietBi DuLieuLienQuan { get; init; }
}

public class ThietBiFormViewModel
{
    public int MaThietBi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên thiết bị")]
    [StringLength(150, ErrorMessage = "Tên thiết bị tối đa 150 ký tự")]
    [Display(Name = "Tên thiết bị")]
    public string TenThietBi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại thiết bị")]
    [Display(Name = "Loại thiết bị")]
    public int? MaLoaiThietBi { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn khu vực")]
    [Display(Name = "Khu vực")]
    public int? MaKhuVuc { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số serial")]
    [StringLength(100, ErrorMessage = "Số serial tối đa 100 ký tự")]
    [Display(Name = "Số serial")]
    public string SoSerial { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngày đưa vào sử dụng")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Ngày đưa vào sử dụng")]
    public DateTime? NgayDuaVaoSuDung { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập chu kỳ bảo trì")]
    [Range(1, 600, ErrorMessage = "Chu kỳ bảo trì phải lớn hơn 0 (tối đa 600 tháng)")]
    [Display(Name = "Chu kỳ bảo trì (tháng)")]
    public int? ChuKyBaoTriThang { get; set; }

    // Để trống = chưa bảo trì lần nào (mốc tính từ ngày đưa vào sử dụng)
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Ngày bảo trì gần nhất")]
    public DateTime? NgayBaoTriGanNhat { get; set; }

    [StringLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    public bool LaThemMoi => MaThietBi == 0;

    [BindNever, ValidateNever]
    public List<SelectListItem> LoaiOptions { get; set; } = new();

    [BindNever, ValidateNever]
    public List<SelectListItem> KhuVucOptions { get; set; } = new();
}
