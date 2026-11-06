// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 2: Quản lý thiết bị
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("ThietBi")]
public class ThietBi
{
    [Key]
    public int MaThietBi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên thiết bị"), StringLength(150)]
    [Display(Name = "Tên thiết bị")]
    public string TenThietBi { get; set; } = string.Empty;

    [Display(Name = "Loại thiết bị")]
    public int MaLoaiThietBi { get; set; }

    [Display(Name = "Khu vực")]
    public int MaKhuVuc { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số serial"), StringLength(100)]
    [Display(Name = "Số serial")]
    public string SoSerial { get; set; } = string.Empty;

    [Column(TypeName = "date"), DataType(DataType.Date)]
    [Display(Name = "Ngày đưa vào sử dụng")]
    public DateTime NgayDuaVaoSuDung { get; set; }

    [Range(1, 600, ErrorMessage = "Chu kỳ bảo trì phải > 0 (tháng)")]
    [Display(Name = "Chu kỳ bảo trì (tháng)")]
    public int ChuKyBaoTriThang { get; set; }

    // Null nếu chưa từng bảo trì, lấy mốc tính từ ngày đưa vào sử dụng
    [Column(TypeName = "date"), DataType(DataType.Date)]
    [Display(Name = "Ngày bảo trì gần nhất")]
    public DateTime? NgayBaoTriGanNhat { get; set; }

    [Display(Name = "Tình trạng")]
    public TinhTrangThietBiEnum TinhTrang { get; set; } = TinhTrangThietBiEnum.DangHoatDong;

    [StringLength(255), Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    // Tính động theo chu kỳ bảo trì, không lưu cột trong DB
    [NotMapped, Display(Name = "Ngày bảo trì tiếp theo")]
    public DateTime NgayBaoTriTiepTheo =>
        (NgayBaoTriGanNhat ?? NgayDuaVaoSuDung).AddMonths(ChuKyBaoTriThang);

    [NotMapped, Display(Name = "Trạng thái hạn bảo trì")]
    public TrangThaiHanBaoTriEnum? TrangThaiHanBaoTri =>
        TinhTrang == TinhTrangThietBiEnum.NgungSuDung
            ? null
            : UiHelper.PhanLoaiHanBaoTri(NgayBaoTriTiepTheo);

    public LoaiThietBi? LoaiThietBi { get; set; }
    public KhuVuc? KhuVuc { get; set; }
    public ICollection<KeHoachBaoTri> KeHoachBaoTris { get; set; } = new List<KeHoachBaoTri>();
    public ICollection<BaoCaoSuCo> BaoCaoSuCos { get; set; } = new List<BaoCaoSuCo>();
    public ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
