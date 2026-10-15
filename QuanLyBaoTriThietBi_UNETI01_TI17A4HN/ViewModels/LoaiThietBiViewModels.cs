// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - ViewModel danh sách Loại thiết bị (kèm số thiết bị đang dùng loại này)
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
