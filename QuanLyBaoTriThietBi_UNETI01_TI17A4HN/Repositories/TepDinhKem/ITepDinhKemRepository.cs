// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Nội dung: STT 53 - Repository Interface cho Tệp đính kèm đa hình (TepDinhKem)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface ITepDinhKemRepository : IRepository<TepDinhKem>
{
    /// Lấy danh sách tệp đính kèm theo loại đối tượng và mã đối tượng (kèm thông tin tài khoản upload)
    Task<List<TepDinhKem>> LayTheoDoiTuongAsync(LoaiDoiTuongDinhKemEnum loaiDoiTuong, int maDoiTuong);

    /// Kiểm tra đối tượng cha có tồn tại trong hệ thống hay không (ràng buộc đa hình logic)
    Task<bool> KiemTraDoiTuongTonTaiAsync(LoaiDoiTuongDinhKemEnum loaiDoiTuong, int maDoiTuong);

    /// Đếm số lượng tệp theo loại và mã đối tượng
    Task<int> DemSoTepAsync(LoaiDoiTuongDinhKemEnum loaiDoiTuong, int maDoiTuong);
}
