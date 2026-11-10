// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý Hệ thống & Danh mục dùng chung - NhatKyDangNhap.cs
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("NhatKyDangNhap")]
public class NhatKyDangNhap
{
    [Key] public int MaNhatKy { get; set; }

    // = null nếu nhập sai tên đăng nhập
    public int? MaTaiKhoan { get; set; }

    [Required, StringLength(50)]
    public string TenDangNhapNhap { get; set; } = string.Empty;

    public DateTime ThoiGian { get; set; }

    public KetQuaDangNhapEnum KetQua { get; set; }

    [StringLength(45)] public string? DiaChiIP { get; set; }

    public TaiKhoan? TaiKhoan { get; set; }
}
