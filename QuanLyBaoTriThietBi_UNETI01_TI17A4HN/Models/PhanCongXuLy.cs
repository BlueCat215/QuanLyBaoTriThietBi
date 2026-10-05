/// 23103100229_Trịnh Trung Hoàng: Module 4: Tạo Model PhanCongXuLy.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

//Khi đổi KTV: KHÔNG UPDATE ban ghi cũ. Set ban ghi cũ = DaKetThuc (SaveChanges TRUOC),
// rồi mới tạo bản ghi mới (unique index chỉ cho 1 ban ghi DangHieuLuc / phieu).
[Table("PhanCongXuLy")]
public class PhanCongXuLy
{
    [Key] public int MaPhanCong { get; set; }

    public int MaPhieuXuLy { get; set; }

    [Display(Name = "Kỹ thuật viên")]
    public int MaKyThuatVien { get; set; }

    [Display(Name = "Ngày phân công")]
    public DateTime NgayPhanCong { get; set; }

    [Display(Name = "Ngày kết thúc phân công")]
    public DateTime? NgayKetThucPhanCong { get; set; }

    [StringLength(500), Display(Name = "Nội dung phân công")]
    public string? NoiDungPhanCong { get; set; }

    [Display(Name = "Trạng thái")]
    public TrangThaiPhanCongEnum TrangThai { get; set; } = TrangThaiPhanCongEnum.DangHieuLuc;

    public PhieuXuLy? PhieuXuLy { get; set; }
    public KyThuatVien? KyThuatVien { get; set; }
}
