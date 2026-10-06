#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("ChiTietXuLy")]
public class ChiTietXuLy
{
    [Key] public int MaChiTiet { get; set; }

    public int MaPhieuXuLy { get; set; }

    [Display(Name = "Ngày cập nhật")]
    public DateTime NgayCapNhat { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung thực hiện"), StringLength(500)]
    [Display(Name = "Nội dung thực hiện")]
    public string NoiDungThucHien { get; set; } = string.Empty;

    // Tên vật tư
    [StringLength(150), Display(Name = "Vật tư / linh kiện")]
    public string? VatTuLinhKien { get; set; }

    public int? MaVatTu { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải > 0")]
    [Display(Name = "Số lượng")]
    public int? SoLuong { get; set; }

    [Column(TypeName = "decimal(18,2)"), Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải >= 0")]
    [Display(Name = "Đơn giá")]
    public decimal? DonGia { get; set; }

    // Tính thành tiền = Số lượng * Đơn giá
    [Column(TypeName = "decimal(18,2)"), Display(Name = "Thành tiền")]
    public decimal? ThanhTien { get; set; }

    [StringLength(255), Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public PhieuXuLy? PhieuXuLy { get; set; }
    public VatTu? VatTu { get; set; }
}
