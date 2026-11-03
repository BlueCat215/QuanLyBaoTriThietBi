// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Nội dung: STT 53 - Service Interface cho Hệ thống Upload tệp đính kèm đa hình (TepDinhKem)
#nullable enable
using Microsoft.AspNetCore.Http;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface ITepDinhKemService
{
    /// Upload một tệp đính kèm an toàn cho đối tượng đa hình
    Task<ServiceResult<TepDinhKemItemViewModel>> UploadTepAsync(
        IFormFile file,
        LoaiDoiTuongDinhKemEnum loaiDoiTuong,
        int maDoiTuong,
        int? maTaiKhoanUpload,
        bool chiChapNhanAnh = true
    );

    /// Upload nhiều tệp đính kèm cùng lúc cho đối tượng
    Task<ServiceResult<List<TepDinhKemItemViewModel>>> UploadNhieuTepAsync(
        IEnumerable<IFormFile> files,
        LoaiDoiTuongDinhKemEnum loaiDoiTuong,
        int maDoiTuong,
        int? maTaiKhoanUpload,
        bool chiChapNhanAnh = true
    );

    /// Lấy danh sách tệp đính kèm theo đối tượng
    Task<List<TepDinhKemItemViewModel>> LayDanhSachTheoDoiTuongAsync(
        LoaiDoiTuongDinhKemEnum loaiDoiTuong,
        int maDoiTuong
    );

    /// Lấy chi tiết một tệp đính kèm
    Task<TepDinhKemItemViewModel?> LayTheoMaAsync(int maTepDinhKem);

    /// Xóa an toàn tệp đính kèm (xóa bản ghi database và tệp vật lý trên đĩa)
    Task<ServiceResult> XoaTepAsync(int maTepDinhKem);

    /// Xóa toàn bộ tệp đính kèm của một đối tượng khi đối tượng đó bị xóa
    Task<ServiceResult> XoaToanBoTheoDoiTuongAsync(
        LoaiDoiTuongDinhKemEnum loaiDoiTuong,
        int maDoiTuong
    );
}
