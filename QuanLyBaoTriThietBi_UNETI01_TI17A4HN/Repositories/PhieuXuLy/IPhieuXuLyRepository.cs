// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Interface Repository Phiếu xử lý và Phân công xử lý
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IPhieuXuLyRepository : IRepository<PhieuXuLy>
{
    // Lấy danh sách phiếu có phân trang, lọc và sắp xếp 
    Task<PagedResult<PhieuXuLyDanhSachRow>> LayPhanTrangAsync(PhieuXuLyBoLoc boLoc);

    // Lấy danh sách phiếu của một kỹ thuật viên (dành cho KTV - "Việc của tôi")
    Task<PagedResult<PhieuXuLyDanhSachRow>> LayPhanTrangTheoKtvAsync(int maKyThuatVien, PhieuXuLyBoLoc boLoc);

    // Lấy chi tiết phiếu xử lý kèm thông tin liên kết
    Task<PhieuXuLy?> LayChiTietAsync(int maPhieuXuLy);

    // LINQ truy vấn KTV đang phụ trách hiện tại của 1 phiếu (TrangThai = DangHieuLuc)
    Task<KyThuatVien?> LayKtvHienTaiAsync(int maPhieuXuLy);

    // LINQ truy vấn bản ghi phân công đang hiệu lực của 1 phiếu
    Task<PhanCongXuLy?> LayPhanCongHienTaiAsync(int maPhieuXuLy);

    // Lấy toàn bộ lịch sử phân công theo phiếu xử lý sắp xếp mới nhất lên đầu
    Task<List<PhanCongXuLyLichSuRow>> LayLichSuPhanCongAsync(int maPhieuXuLy);

    // Thêm phân công xử lý mới
    Task ThemPhanCongAsync(PhanCongXuLy phanCong);

    // Kiểm tra kỹ thuật viên có tồn tại và đang hoạt động không
    Task<bool> KtvDangHoatDongAsync(int maKyThuatVien);

    // Kiểm tra xem báo cáo sự cố hoặc kế hoạch bảo trì đã có phiếu chưa kết thúc hay chưa
    Task<bool> DaCoPhieuDangHoatDongChoBaoCaoAsync(int maBaoCao);
    Task<bool> DaCoPhieuDangHoatDongChoKeHoachAsync(int maKeHoach);

    // Lấy nguồn báo cáo sự cố để chọn khi tạo phiếu
    Task<List<(int Ma, string MoTa)>> LayBaoCaoChoChonAsync();

    // Lấy nguồn kế hoạch bảo trì để chọn khi tạo phiếu
    Task<List<(int Ma, string MoTa)>> LayKeHoachChoChonAsync();

    //Lấy danh sách KTV đang hoạt động
    Task<List<(int Ma, string Ten)>> LayKtvDangHoatDongChoChonAsync();

    // Tìm KTV theo MaTaiKhoan (để phục vụ trang "Việc của tôi")
    Task<KyThuatVien?> LayKtvTheoTaiKhoanAsync(int maTaiKhoan);
}
