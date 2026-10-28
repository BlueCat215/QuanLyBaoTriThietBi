// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Interface Service Phiếu xử lý và Phân công xử lý (bổ sung STT 39 Bắt đầu xử lý)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IPhieuXuLyService
{
    // Lấy danh sách phiếu có phân trang, lọc và sắp xếp
    Task<PhieuXuLyIndexViewModel> LayDanhSachAsync(PhieuXuLyBoLoc boLoc);

    // Lấy danh sách công việc của kỹ thuật viên hiện tại
    Task<PhieuXuLyIndexViewModel> LayViecCuaKtvAsync(int maTaiKhoan, PhieuXuLyBoLoc boLoc);

    // Lấy chi tiết phiếu xử lý kèm thông tin liên quan
    Task<PhieuXuLyDetailsViewModel?> LayChiTietAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap, bool laAdmin);

    // Tạo phiếu xử lý mới gắn đúng nguồn công việc (BaoCaoSuCo hoặc KeHoachBaoTri)
    Task<ServiceResult<int>> TaoPhieuXuLyAsync(TaoPhieuXuLyViewModel model);

    // Chuẩn bị dữ liệu form tạo phiếu mới
    Task<TaoPhieuXuLyViewModel> ChuanBiFormTaoPhieuAsync(int? maBaoCao, int? maKeHoach);

    // Phân công xử lý lần đầu
    Task<ServiceResult> PhanCongXuLyAsync(PhanCongXuLyFormViewModel model);

    // Đổi kỹ thuật viên phụ trách, kết thúc phân công cũ, tạo phân công mới lưu lịch sử
    Task<ServiceResult> DoiKyThuatVienAsync(DoiKyThuatVienFormViewModel model);

    // LINQ truy vấn kỹ thuật viên đang phụ trách hiện tại
    Task<KyThuatVien?> LayKtvHienTaiAsync(int maPhieuXuLy);

    // STT 39: Bắt đầu xử lý phiếu (chỉ KTV đang được phân công hoặc Admin)
    Task<ServiceResult> BatDauXuLyAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap, bool laAdmin);

    // Hủy phiếu xử lý
    Task<ServiceResult> HuyPhieuXuLyAsync(int maPhieuXuLy, string? lyDoHuy);
}
