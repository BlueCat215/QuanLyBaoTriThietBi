// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Interface Repository Kỹ thuật viên
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IKyThuatVienRepository : IRepository<KyThuatVien>
{
    /// Tìm theo họ tên / chuyên môn / SĐT / email / tên đăng nhập (LINQ) + lọc trạng thái.
    Task<List<KyThuatVienRow>> TimKiemAsync(string? tuKhoa, bool? trangThai);

    Task<bool> EmailDaTonTaiAsync(string email, int? boQuaId = null);

    /// Đếm phân công của KTV; chiDangHieuLuc = true chỉ đếm bản ghi DangHieuLuc.
    Task<int> DemPhanCongAsync(int maKyThuatVien, bool chiDangHieuLuc);

    Task<TaiKhoan?> LayTaiKhoanAsync(int maTaiKhoan);
    Task<bool> TaiKhoanDaGanHoSoKhacAsync(int maTaiKhoan, int? boQuaMaKyThuatVien = null);

    /// Tài khoản vai trò KyThuatVien, đang hoạt động, chưa gắn hồ sơ nào (giữ lại tài khoản hiện tại khi sửa).
    Task<List<(int Ma, string Ten)>> LayTaiKhoanChoChonAsync(int? giuLaiMa);

    /// KTV đang hoạt động - dùng cho chức năng Phân công xử lý (việc 35).
    Task<List<(int Ma, string Ten)>> LayDangHoatDongAsync();
}
