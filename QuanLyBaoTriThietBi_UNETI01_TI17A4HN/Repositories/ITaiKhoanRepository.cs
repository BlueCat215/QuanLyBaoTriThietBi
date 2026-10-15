// Họ tên: <điền họ tên>
// MSSV: <điền MSSV>
// Nội dung: Module 1 - Interface Repository Tài khoản
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface ITaiKhoanRepository : IRepository<TaiKhoan>
{
    Task<List<TaiKhoanRow>> TimKiemAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai);
    Task<bool> TenDangNhapDaTonTaiAsync(string tenDangNhap, int? boQuaId = null);

    /// <summary>Số quản trị viên đang hoạt động (có thể loại trừ 1 tài khoản).</summary>
    Task<int> DemAdminHoatDongAsync(int? boQuaId = null);

    /// <summary>Tài khoản đã phát sinh dữ liệu (báo cáo, nhật ký đăng nhập, thông báo, tệp, hồ sơ kỹ thuật viên)?</summary>
    Task<bool> CoDuLieuLienQuanAsync(int maTaiKhoan);

    Task<bool> KhuVucDangHoatDongAsync(int maKhuVuc);

    /// <summary>Khu vực đang hoạt động để chọn; giữ lại khu vực hiện tại của tài khoản dù đã ngừng.</summary>
    Task<List<KhuVucOption>> LayKhuVucChoChonAsync(int? giuLaiMa);
}
