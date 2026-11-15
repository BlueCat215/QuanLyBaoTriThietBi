// Họ tên: Nguyễn Khắc Hiếu - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 5: Quản lý Kho vật tư - IVatTuRepository.cs
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IVatTuRepository : IRepository<VatTu>
{
    Task<(List<VatTu> Items, int TotalCount)> LayDanhSachAsync(VatTuFilterViewModel filter);

    Task<bool> TenDaTonTaiAsync(string tenVatTu, int? loaiTruMaVatTu = null);

    Task<int> DemSoLanSuDungAsync(int maVatTu);

    Task<(int TongSo, int SapHet, int HetHang, decimal TongGiaTri)> LayThongKeKhoAsync();

    Task<List<VatTu>> LayTatCaDeXuatExcelAsync(VatTuFilterViewModel filter);

    Task<VatTu?> LayTheoMaTrackingAsync(int maVatTu);
}
