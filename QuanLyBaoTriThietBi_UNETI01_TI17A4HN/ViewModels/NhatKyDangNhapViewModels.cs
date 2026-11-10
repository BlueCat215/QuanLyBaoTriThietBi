// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý hệ thống & Danh mục - NhatKyDangNhapViewModels
#nullable enable
using System.ComponentModel.DataAnnotations;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

/// <summary>
/// Dòng dữ liệu hiển thị một bản ghi nhật ký đăng nhập trên bảng tra cứu.
/// </summary>
public record NhatKyDangNhapRow(
    int MaNhatKy,
    int? MaTaiKhoan,
    string TenDangNhapNhap,
    string? HoTen,
    VaiTroEnum? VaiTro,
    DateTime ThoiGian,
    KetQuaDangNhapEnum KetQua,
    string? DiaChiIP
);

/// <summary>
/// Bộ lọc tìm kiếm và phân trang nhật ký đăng nhập.
/// </summary>
public class NhatKyDangNhapBoLoc
{
    [Display(Name = "Từ khóa")]
    public string? TuKhoa { get; set; }

    [Display(Name = "Kết quả")]
    public KetQuaDangNhapEnum? KetQua { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Từ ngày")]
    public DateTime? TuNgay { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Đến ngày")]
    public DateTime? DenNgay { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public bool CoBoLoc =>
        !string.IsNullOrWhiteSpace(TuKhoa)
        || KetQua.HasValue
        || TuNgay.HasValue
        || DenNgay.HasValue;

    public Dictionary<string, string> ToRouteValues(int page)
    {
        var dict = new Dictionary<string, string> { ["page"] = page.ToString() };
        if (!string.IsNullOrWhiteSpace(TuKhoa))
            dict["tuKhoa"] = TuKhoa;
        if (KetQua.HasValue)
            dict["ketQua"] = KetQua.Value.ToString();
        if (TuNgay.HasValue)
            dict["tuNgay"] = TuNgay.Value.ToString("yyyy-MM-dd");
        if (DenNgay.HasValue)
            dict["denNgay"] = DenNgay.Value.ToString("yyyy-MM-dd");
        return dict;
    }
}

/// <summary>
/// Thống kê nhanh số liệu các lượt đăng nhập phục vụ các thẻ KPI trên giao diện.
/// </summary>
public class NhatKyDangNhapThongKe
{
    public int TongSo { get; set; }
    public int SoThanhCong { get; set; }
    public int SoSaiMatKhau { get; set; }
    public int SoBiKhoa { get; set; }
    public int SoKhongTonTai { get; set; }
    public int SoThatBai => SoSaiMatKhau + SoBiKhoa + SoKhongTonTai;
}

/// <summary>
/// ViewModel chính cho trang Index của Nhật ký đăng nhập.
/// </summary>
public class NhatKyDangNhapIndexViewModel
{
    public NhatKyDangNhapBoLoc BoLoc { get; set; } = new();
    public PagedResult<NhatKyDangNhapRow> PagedItems { get; set; } = new();
    public NhatKyDangNhapThongKe ThongKe { get; set; } = new();
}
