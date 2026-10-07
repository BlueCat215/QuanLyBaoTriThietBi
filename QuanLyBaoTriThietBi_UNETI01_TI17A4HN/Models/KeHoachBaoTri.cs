//23103100207_PhamVietHoan:Module 3:Tạo Model KeHoachBaoTri.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("KeHoachBaoTri")]
public class KeHoachBaoTri
{
    [Key] public int MaKeHoach { get; set; }

    [Display(Name = "Thiết bị")]
    public int MaThietBi { get; set; }

    [Column(TypeName = "date"), DataType(DataType.Date)]
    [Display(Name = "Ngày dự kiến")]
    public DateTime NgayDuKien { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bảo trì"), StringLength(500)]
    [Display(Name = "Nội dung bảo trì")]
    public string NoiDungBaoTri { get; set; } = string.Empty;

    [Display(Name = "Mức độ ưu tiên")]
    public MucDoUuTienEnum MucDoUuTien { get; set; } = MucDoUuTienEnum.TrungBinh;

    [Display(Name = "Trạng thái")]
    public TrangThaiKeHoachEnum TrangThai { get; set; } = TrangThaiKeHoachEnum.ChoThucHien;

    [StringLength(500), Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public ThietBi? ThietBi { get; set; }
    public ICollection<PhieuXuLy> PhieuXuLys { get; set; } = new List<PhieuXuLy>();
}