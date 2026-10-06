#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("TepDinhKem")]
public class TepDinhKem
{
    [Key] public int MaTepDinhKem { get; set; }

    public LoaiDoiTuongDinhKemEnum LoaiDoiTuong { get; set; }
    public int MaDoiTuong { get; set; }

    [Required, StringLength(255)]
    public string DuongDanFile { get; set; } = string.Empty;

    [StringLength(255)] public string? TenFileGoc { get; set; }

    public DateTime NgayUpload { get; set; }

    public int? MaTaiKhoanUpload { get; set; }

    public TaiKhoan? TaiKhoanUpload { get; set; }
}
