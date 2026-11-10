// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý hệ thống & Danh mục - Thực thể Khu vực (KhuVuc)
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("KhuVuc")]
public class KhuVuc
{
    [Key] public int MaKhuVuc { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên khu vực"), StringLength(100)]
    [Display(Name = "Tên khu vực")]
    public string TenKhuVuc { get; set; } = string.Empty;

    [StringLength(255), Display(Name = "Vị trí")]
    public string? ViTri { get; set; }

    [StringLength(100), Display(Name = "Người phụ trách")]
    public string? NguoiPhuTrach { get; set; }

    [StringLength(255), Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Display(Name = "Đang sử dụng")]
    public bool TrangThai { get; set; } = true;

    public ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
    public ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
