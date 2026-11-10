// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý hệ thống & Danh mục - KhuVucViewModels
#nullable enable
namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public record KhuVucRow(
    int MaKhuVuc,
    string TenKhuVuc,
    string? ViTri,
    string? NguoiPhuTrach,
    bool TrangThai,
    int SoThietBi,
    int SoTaiKhoan);

public class KhuVucIndexViewModel
{
    public List<KhuVucRow> Items { get; set; } = new();
    public string? TuKhoa { get; set; }
    public bool? TrangThai { get; set; }
    public bool CoBoLoc => !string.IsNullOrWhiteSpace(TuKhoa) || TrangThai.HasValue;
}
