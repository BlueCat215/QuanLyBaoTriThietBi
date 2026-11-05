// Họ tên: Nguyễn Khắc Hiếu
// MSSV: 23103100225
// Nội dung: Module 5 - STT 57: Interface Repository Quản lý kho vật tư
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
