// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - ViewModel Kế hoạch bảo trì (danh sách, chi tiết vòng đời, form Thêm/Sửa)
// Nội dung: Module 3 - ViewModel Kế hoạch bảo trì (danh sách, tìm kiếm + lọc + sắp xếp + phân trang, form Thêm/Sửa/Nghiệm thu)
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public enum KeHoachBaoTriSapXep
{
    NgayDuKienGanNhat,
    NgayDuKienXaNhat,
    UuTienCaoNhat,
    UuTienThapNhat,
    TenThietBiAZ,
    TenThietBiZA,
    MaKeHoachMoiNhat,
}

public class KeHoachBaoTriBoLoc
{
    public string? TuKhoa { get; set; }
    public TrangThaiKeHoachEnum? TrangThai { get; set; }
    public MucDoUuTienEnum? MucDo { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public KeHoachBaoTriSapXep SapXep { get; set; } = KeHoachBaoTriSapXep.NgayDuKienGanNhat;

    // Phân trang
    public int Trang { get; set; } = 1;
    public int SoBanGhiMoiTrang { get; set; } = 10;

    /// Có điều kiện LỌC nào đang bật không (không tính Sắp xếp và Phân trang).
    public bool CoBoLoc =>
        !string.IsNullOrWhiteSpace(TuKhoa)
        || TrangThai.HasValue
        || MucDo.HasValue
        || TuNgay.HasValue
        || DenNgay.HasValue;

    /// "Từ ngày" lớn hơn "Đến ngày": khoảng ngày bị bỏ qua và trang hiện cảnh báo.
    public bool KhoangNgayKhongHopLe =>
        TuNgay.HasValue
        && DenNgay.HasValue
        && TuNgay.Value.Date > DenNgay.Value.Date;

    /// Tạo từ điển route values để gắn vào link phân trang, giữ nguyên các điều kiện tìm kiếm, lọc, sắp xếp
    public Dictionary<string, string> ToRouteValues(int? trang = null)
    {
        var route = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(TuKhoa))
            route[nameof(TuKhoa)] = TuKhoa;

        if (TrangThai.HasValue)
            route[nameof(TrangThai)] = TrangThai.Value.ToString();

        if (MucDo.HasValue)
            route[nameof(MucDo)] = MucDo.Value.ToString();

        if (TuNgay.HasValue)
            route[nameof(TuNgay)] = TuNgay.Value.ToString("yyyy-MM-dd");

        if (DenNgay.HasValue)
            route[nameof(DenNgay)] = DenNgay.Value.ToString("yyyy-MM-dd");

        if (SapXep != KeHoachBaoTriSapXep.NgayDuKienGanNhat)
            route[nameof(SapXep)] = SapXep.ToString();

        if (SoBanGhiMoiTrang != 10)
            route[nameof(SoBanGhiMoiTrang)] = SoBanGhiMoiTrang.ToString();

        var p = trang ?? Trang;
        if (p > 1)
            route[nameof(Trang)] = p.ToString();

        return route;
    }
}

public static class KeHoachBaoTriBoLocText
{
    public static string SapXep(KeHoachBaoTriSapXep v) =>
        v switch
        {
            KeHoachBaoTriSapXep.NgayDuKienGanNhat => "Ngày dự kiến: gần nhất",
            KeHoachBaoTriSapXep.NgayDuKienXaNhat => "Ngày dự kiến: xa nhất",
            KeHoachBaoTriSapXep.UuTienCaoNhat => "Ưu tiên: cao → thấp",
            KeHoachBaoTriSapXep.UuTienThapNhat => "Ưu tiên: thấp → cao",
            KeHoachBaoTriSapXep.TenThietBiAZ => "Thiết bị A → Z",
            KeHoachBaoTriSapXep.TenThietBiZA => "Thiết bị Z → A",
            KeHoachBaoTriSapXep.MaKeHoachMoiNhat => "Mã KH mới nhất",
            _ => v.ToString(),
        };
}

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
    // Service kiểm tra lại đầy đủ (còn phiếu xử lý đang hoạt động thì không hủy được)

    // Có thể thực hiện nghiệm thu / hoàn thành kế hoạch
    public bool CoTheHoanThanh =>
        TrangThai is TrangThaiKeHoachEnum.DangThucHien or TrangThaiKeHoachEnum.ChoNghiemThu;
}

public class KeHoachBaoTriIndexViewModel
{
    /// Danh sách kết quả theo phân trang (chứa metadata Page, TotalPages, TotalItems)
    public PagedResult<KeHoachRow> PagedItems { get; set; } = new();

    /// Danh sách phần tử của trang hiện tại (tương thích với Model.Items)
    public List<KeHoachRow> Items => PagedItems.Items;

    /// Điều kiện Tìm kiếm + Lọc + Sắp xếp + Phân trang hiện tại
    public KeHoachBaoTriBoLoc BoLoc { get; set; } = new();

    public string? TuKhoa => BoLoc.TuKhoa;
    public TrangThaiKeHoachEnum? TrangThai => BoLoc.TrangThai;
    public MucDoUuTienEnum? MucDo => BoLoc.MucDo;
    public DateTime? TuNgay => BoLoc.TuNgay;
    public DateTime? DenNgay => BoLoc.DenNgay;
    public KeHoachBaoTriSapXep SapXep => BoLoc.SapXep;

    public bool CoBoLoc => BoLoc.CoBoLoc;

    /// Thông báo khi có điều kiện lọc không hợp lệ (ví dụ: khoảng ngày bị đảo).
    public string? CanhBao { get; set; }
}

public class HoanThanhKeHoachViewModel
{
    public int MaKeHoach { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày hoàn thành thực tế")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày hoàn thành thực tế")]
    public DateTime NgayHoanThanh { get; set; } = DateTime.Today;

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
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
