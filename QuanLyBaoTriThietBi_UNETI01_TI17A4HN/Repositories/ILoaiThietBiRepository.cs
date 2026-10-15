// Họ tên:  Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Interface Repository Loại thiết bị
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
