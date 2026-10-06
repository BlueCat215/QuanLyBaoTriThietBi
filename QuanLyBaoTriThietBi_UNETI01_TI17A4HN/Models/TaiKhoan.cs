///2310310023103100303_Nguyễn Văn Hoàng: Module 1: Tạo Model TaiKhoan.cs
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("TaiKhoan")]
public class TaiKhoan
{
    [Key] public int MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập"), StringLength(50)]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string MatKhau { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100)]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email"), StringLength(100), EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Vai trò")]
    public VaiTroEnum VaiTro { get; set; }

    [Display(Name = "Khu vực")]
    public int? MaKhuVuc { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool TrangThai { get; set; } = true;

    [StringLength(255)] public string? MatKhauSalt { get; set; }

    public int SoLanDangNhapSai { get; set; }

    public DateTime? KhoaDenNgay { get; set; }

    public DateTime NgayTaoTaiKhoan { get; set; }

    public DateTime? LanDangNhapCuoi { get; set; }

    public KhuVuc? KhuVuc { get; set; }
    public KyThuatVien? KyThuatVien { get; set; }
    public ICollection<BaoCaoSuCo> BaoCaoSuCos { get; set; } = new List<BaoCaoSuCo>();
    public ICollection<NhatKyDangNhap> NhatKyDangNhaps { get; set; } = new List<NhatKyDangNhap>();
    public ICollection<TepDinhKem> TepDinhKems { get; set; } = new List<TepDinhKem>();
    public ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
