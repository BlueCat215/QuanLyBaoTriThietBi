#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("VatTu")]
public class VatTu
{
    [Key] public int MaVatTu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên vật tư"), StringLength(150)]
    [Display(Name = "Tên vật tư")]
    public string TenVatTu { get; set; } = string.Empty;

    [Required, StringLength(30), Display(Name = "Đơn vị tính")]
    public string DonViTinh { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)"), Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải >= 0")]
    [Display(Name = "Đơn giá mặc định")]
    public decimal DonGiaMacDinh { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn phải >= 0")]
    [Display(Name = "Số lượng tồn")]
    public int SoLuongTon { get; set; }

    [Display(Name = "Đang sử dụng")]
    public bool TrangThai { get; set; } = true;

    public ICollection<ChiTietXuLy> ChiTietXuLys { get; set; } = new List<ChiTietXuLy>();
}
