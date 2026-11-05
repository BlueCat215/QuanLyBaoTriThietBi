// Họ tên: Nguyễn Khắc Hiếu
// MSSV: 23103100225
// Nội dung: Module 5 - STT 57: Interface Service Quản lý kho vật tư
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IVatTuService
{
    Task<VatTuIndexViewModel> LayDanhSachAsync(VatTuFilterViewModel filter, bool laAdmin);

    Task<VatTuFormViewModel?> LayDeSuaAsync(int maVatTu);

    Task<ServiceResult<int>> ThemAsync(VatTuFormViewModel vm);

    Task<ServiceResult> SuaAsync(VatTuFormViewModel vm);

    Task<ServiceResult> XoaAsync(int maVatTu);

    Task<ServiceResult> ChuyenTrangThaiAsync(int maVatTu);

    Task<byte[]> XuatExcelAsync(VatTuFilterViewModel filter);
}
