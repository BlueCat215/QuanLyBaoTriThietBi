// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - ViewModel danh sách Khu vực (kèm số thiết bị, số tài khoản thuộc khu vực)
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
