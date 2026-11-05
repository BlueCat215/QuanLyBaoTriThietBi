// Họ tên: Nguyễn Minh Đức (Module 2), Trịnh Trung Hoàng (MSSV: 23103100229 - Module 4) - Lớp: TI17A4HN - Nhóm: UNETI01
// Nội dung: STT 53 & STT 54 - ViewModel phục vụ hệ thống Upload tệp đính kèm đa hình và Bộ sưu tập ảnh thiết bị
#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public class TepDinhKemItemViewModel
{
    public int MaTepDinhKem { get; set; }
    public LoaiDoiTuongDinhKemEnum LoaiDoiTuong { get; set; }
    public int MaDoiTuong { get; set; }
    public string DuongDanFile { get; set; } = string.Empty;
    public string TenFileGoc { get; set; } = string.Empty;
    public DateTime NgayUpload { get; set; }
    public int? MaTaiKhoanUpload { get; set; }
    public string? TenNguoiUpload { get; set; }

    /// Kiểm tra tệp có phải là định dạng hình ảnh hay không
    public bool LaHinhAnh
    {
        get
        {
            var ext = System.IO.Path.GetExtension(DuongDanFile).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" or ".svg";
        }
    }
}

public class UploadTepDinhKemViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn loại đối tượng đính kèm")]
    public LoaiDoiTuongDinhKemEnum LoaiDoiTuong { get; set; }

    [Required(ErrorMessage = "Mã đối tượng không hợp lệ")]
    public int MaDoiTuong { get; set; }

    [Display(Name = "Tệp đính kèm")]
    public List<IFormFile> DanhSachTep { get; set; } = new();

    public string? ReturnUrl { get; set; }
}
