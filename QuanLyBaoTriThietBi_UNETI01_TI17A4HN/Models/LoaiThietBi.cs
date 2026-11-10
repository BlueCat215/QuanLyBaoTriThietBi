// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý hệ thống & Danh mục - Thực thể Loại thiết bị (LoaiThietBi)
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("LoaiThietBi")]
public class LoaiThietBi
{
    [Key] public int MaLoaiThietBi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên loại thiết bị"), StringLength(100)]
    [Display(Name = "Tên loại thiết bị")]
    public string TenLoaiThietBi { get; set; } = string.Empty;

    [Range(1, 600, ErrorMessage = "Chu kỳ bảo trì phải > 0 (tháng)")]
    [Display(Name = "Chu kỳ bảo trì mặc định (tháng)")]
    public int ChuKyBaoTriMacDinhThang { get; set; }

    [StringLength(255), Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Display(Name = "Đang sử dụng")]
    public bool TrangThai { get; set; } = true;

    public ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
}
