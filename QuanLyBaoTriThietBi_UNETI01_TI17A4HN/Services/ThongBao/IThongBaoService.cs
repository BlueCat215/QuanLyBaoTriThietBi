// Họ tên: Phạm Việt Hoàn - Nguyễn Khắc Hiếu
// MSSV: 23103100207 - 23103100225
// Nội dung: Module 3 & 5 - STT 59: Interface Service Thông báo nội bộ (In-App Notification)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IThongBaoService
{
    /// Lấy số lượng chưa đọc và 10 thông báo mới nhất cho Popup chuông Header
    Task<ThongBaoNhanhViewModel> LayThongBaoNhanhAsync(int maTaiKhoan);

    /// Lấy danh sách toàn bộ thông báo phân trang cho trang Index
    Task<ThongBaoIndexViewModel> LayDanhSachAsync(int maTaiKhoan, bool? chiChuaDoc, int trang = 1);

    /// Đánh dấu 1 thông báo là đã đọc
    Task<bool> DanhDauDaDocAsync(int maThongBao, int maTaiKhoan);

    /// Đánh dấu tất cả thông báo của tài khoản là đã đọc
    Task<int> DanhDauTatCaDaDocAsync(int maTaiKhoan);

    /// Tạo 1 thông báo hệ thống gửi cho một tài khoản cụ thể
    Task TaoThongBaoHeThongAsync(int maTaiKhoanNhan, string noiDung, int? maThietBi = null);

    /// Gửi thông báo hệ thống tới tất cả Quản trị viên (Admin)
    Task TaoThongBaoChoAdminAsync(string noiDung, int? maThietBi = null);

    /// Gửi thông báo hệ thống tới Admin và Kỹ thuật viên
    Task TaoThongBaoChoAdminVaKtvAsync(string noiDung, int? maThietBi = null, int? maKhuVuc = null);

    /// Gửi thông báo hệ thống cho Kỹ thuật viên phụ trách theo MaKyThuatVien
    Task TaoThongBaoChoKtvAsync(int maKyThuatVien, string noiDung, int? maThietBi = null);
}
