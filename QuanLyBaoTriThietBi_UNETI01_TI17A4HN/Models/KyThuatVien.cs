//23103100229_Trịnh Trung Hoàng: Module 4: Tạo Model KyThuatVien.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("KyThuatVien")]
public class KyThuatVien
{
    [Key] public int MaKyThuatVien { get; set; }

    //FK 0..1 toi TaiKhoan (unique có điều kiện WHERE NOT NULL).
    public int? MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100)]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(150), Display(Name = "Chuyên môn")]
    public string? ChuyenMon { get; set; }

    [StringLength(15), Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [StringLength(100), EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    // false = ngừng hoạt động, không đươc nhận phân công mới.
    [Display(Name = "Đang hoạt động")]
    public bool TrangThai { get; set; } = true;

    public TaiKhoan? TaiKhoan { get; set; }
    public ICollection<PhanCongXuLy> PhanCongXuLys { get; set; } = new List<PhanCongXuLy>();
}
