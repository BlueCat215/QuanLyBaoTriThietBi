// Họ tên: Phạm Việt Hoàn - Nguyễn Khắc Hiếu
// MSSV: 23103100207 - 23103100225
// Nội dung: Module 3 & 5 - STT 59: Interface Repository Thông báo nội bộ
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IThongBaoRepository : IRepository<ThongBao>
{
    /// Đếm số thông báo hệ thống chưa đọc của một tài khoản
    Task<int> DemSoChuaDocAsync(int maTaiKhoan);

    /// Lấy danh sách thông báo mới nhất cho Popup chuông thông báo trên Header
    Task<List<ThongBaoItemDto>> LayDanhSachGanNhatAsync(int maTaiKhoan, int soLuong = 10);

    /// Lấy danh sách thông báo phân trang cho trang Index
    Task<PagedResult<ThongBaoItemDto>> LayPhanTrangAsync(int maTaiKhoan, bool? chiChuaDoc, int trang = 1, int soBanGhi = 15);

    /// Đánh dấu một thông báo đã đọc
    Task<bool> DanhDauDaDocAsync(int maThongBao, int maTaiKhoan);

    /// Đánh dấu tất cả thông báo hệ thống của tài khoản là đã đọc
    Task<int> DanhDauTatCaDaDocAsync(int maTaiKhoan);

    /// Thêm thông báo mới
    Task ThemThongBaoAsync(ThongBao thongBao);

    /// Thêm nhiều thông báo cùng lúc
    Task ThemNhieuThongBaoAsync(IEnumerable<ThongBao> danhSach);
}
