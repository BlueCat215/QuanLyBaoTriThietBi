/// 23103100229_Trịnh Trung Hoàng: Module 4: Tạo Model BaoCaoSuCo.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("BaoCaoSuCo")]
public class BaoCaoSuCo
{
    [Key] public int MaBaoCao { get; set; }

    [Display(Name = "Thiết bị")]
    public int MaThietBi { get; set; }

    public int MaTaiKhoanBaoCao { get; set; }

    // Hệ thống tự gán (DateTime.Now), không cho nhập tay =
    [Display(Name = "Ngày báo cáo")]
    public DateTime NgayBaoCao { get; set; }

    [Required(ErrorMessage = "Vui lòng mô tả sự cố"), StringLength(500)]
    [Display(Name = "Mô tả sự cố")]
    public string MoTaSuCo { get; set; } = string.Empty;

    [Display(Name = "Mức độ sự cố")]
    public MucDoSuCoEnum MucDoSuCo { get; set; } = MucDoSuCoEnum.TrungBinh;

    [Display(Name = "Trạng thái")]
    public TrangThaiBaoCaoEnum TrangThai { get; set; } = TrangThaiBaoCaoEnum.MoiBao;

    [StringLength(500), Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public ThietBi? ThietBi { get; set; }
    public TaiKhoan? TaiKhoanBaoCao { get; set; }
    public ICollection<PhieuXuLy> PhieuXuLys { get; set; } = new List<PhieuXuLy>();
}
