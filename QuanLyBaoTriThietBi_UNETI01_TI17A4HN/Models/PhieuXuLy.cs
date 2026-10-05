/// 23103100229_Trịnh Trung Hoàng: Module 4: Tạo Model PhieuXuLy.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

[Table("PhieuXuLy")]
public class PhieuXuLy
{
    [Key] public int MaPhieuXuLy { get; set; }
    // Có giá trị khi LoaiXuLy = XuLySuCo.</summary>
    public int? MaBaoCao { get; set; }

    //Có giá trị khi LoaiXuLy = BaoTriDinhKy.</summary>
    public int? MaKeHoach { get; set; }

    [Display(Name = "Loại xử lý")]
    public LoaiXuLyEnum LoaiXuLy { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime NgayTao { get; set; }

    [Display(Name = "Ngày bắt đầu")]
    public DateTime? NgayBatDau { get; set; }

    [Display(Name = "Ngày hoàn thành")]
    public DateTime? NgayHoanThanh { get; set; }

    [Display(Name = "Trạng thái")]
    public TrangThaiPhieuXuLyEnum TrangThai { get; set; } = TrangThaiPhieuXuLyEnum.ChoPhanCong;

    [StringLength(500), Display(Name = "Kết quả xử lý")]
    public string? KetQuaXuLy { get; set; }

    //Tinh dòng = SUM(ThanhTien) của ChiTietXuLy, KHÔNG lưu cột riêng.
    // Nếu dùng trong truy vấn LINQ, phai Include(ChiTietXuLys) hoặc tính bằng Sum() trong query
    [NotMapped, Display(Name = "Tổng chi phí")]
    public decimal TongChiPhi => ChiTietXuLys.Sum(c => c.ThanhTien ?? 0m);

    public BaoCaoSuCo? BaoCaoSuCo { get; set; }
    public KeHoachBaoTri? KeHoachBaoTri { get; set; }
    public ICollection<PhanCongXuLy> PhanCongXuLys { get; set; } = new List<PhanCongXuLy>();
    public ICollection<ChiTietXuLy> ChiTietXuLys { get; set; } = new List<ChiTietXuLy>();
}
