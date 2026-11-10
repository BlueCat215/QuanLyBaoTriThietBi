// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý hệ thống & Danh mục - LoaiThietBiViewModels
#nullable enable
namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public record LoaiThietBiRow(
    int MaLoaiThietBi,
    string TenLoaiThietBi,
    int ChuKyBaoTriMacDinhThang,
    string? MoTa,
    bool TrangThai,
    int SoThietBi);

public class LoaiThietBiIndexViewModel
{
    public List<LoaiThietBiRow> Items { get; set; } = new();
    public string? TuKhoa { get; set; }
    public bool? TrangThai { get; set; }
    public bool CoBoLoc => !string.IsNullOrWhiteSpace(TuKhoa) || TrangThai.HasValue;
}
