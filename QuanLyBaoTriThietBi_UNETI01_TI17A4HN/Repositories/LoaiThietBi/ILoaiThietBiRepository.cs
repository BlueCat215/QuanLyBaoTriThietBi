// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý Hệ thống & Danh mục dùng chung - ILoaiThietBiRepository.cs
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface ILoaiThietBiRepository : IRepository<LoaiThietBi>
{
    Task<List<LoaiThietBiRow>> TimKiemAsync(string? tuKhoa, bool? trangThai);
    Task<bool> TenDaTonTaiAsync(string ten, int? boQuaId = null);
    Task<int> DemThietBiAsync(int maLoaiThietBi);
}
