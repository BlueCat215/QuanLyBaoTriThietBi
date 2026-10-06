#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("ThongBao")]
public class ThongBao
{
    [Key] public int MaThongBao { get; set; }

    public KenhThongBaoEnum Kenh { get; set; }

    public int MaTaiKhoanNhan { get; set; }

    public int? MaThietBi { get; set; }

    [Required, StringLength(500)]
    public string NoiDung { get; set; } = string.Empty;

    public DateTime NgayTao { get; set; }

    public DateTime? NgayGui { get; set; }

    public TrangThaiGuiThongBaoEnum TrangThai { get; set; } = TrangThaiGuiThongBaoEnum.ChoGui;

    public bool DaDoc { get; set; }

    public TaiKhoan? TaiKhoanNhan { get; set; }
    public ThietBi? ThietBi { get; set; }
}
