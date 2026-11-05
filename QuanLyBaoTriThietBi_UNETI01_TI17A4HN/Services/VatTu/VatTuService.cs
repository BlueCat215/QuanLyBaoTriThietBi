// Họ tên: Nguyễn Khắc Hiếu
// MSSV: 23103100225
// Nội dung: Module 5 - STT 57: Triển khai Service Quản lý kho vật tư (CRUD, validation, cảnh báo tồn kho, xuất Excel)
#nullable enable
using ClosedXML.Excel;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public record VatTuExportRow(
    string MaVatTuText,
    string TenVatTu,
    string DonViTinh,
    decimal DonGiaMacDinh,
    int SoLuongTon,
    decimal TongGiaTri,
    string TinhTrangTon,
    string TrangThaiText
);

public class VatTuService : IVatTuService
{
    private readonly IVatTuRepository _repo;

    public VatTuService(IVatTuRepository repo)
    {
        _repo = repo;
    }

    public async Task<VatTuIndexViewModel> LayDanhSachAsync(VatTuFilterViewModel filter, bool laAdmin)
    {
        var (items, total) = await _repo.LayDanhSachAsync(filter);
        var (tongSo, sapHet, hetHang, tongGiaTri) = await _repo.LayThongKeKhoAsync();

        var viewItems = items.Select(v => new VatTuItemViewModel
        {
            MaVatTu = v.MaVatTu,
            TenVatTu = v.TenVatTu,
            DonViTinh = v.DonViTinh,
            DonGiaMacDinh = v.DonGiaMacDinh,
            SoLuongTon = v.SoLuongTon,
            TrangThai = v.TrangThai,
            SoLanSuDung = v.ChiTietXuLys.Count
        }).ToList();

        return new VatTuIndexViewModel
        {
            Items = viewItems,
            Filter = filter,
            TotalItems = total,
            TongSoVatTu = tongSo,
            SoVatTuSapHet = sapHet,
            SoVatTuHetHang = hetHang,
            TongGiaTriTonKho = tongGiaTri,
            LaAdmin = laAdmin
        };
    }

    public async Task<VatTuFormViewModel?> LayDeSuaAsync(int maVatTu)
    {
        var vt = await _repo.GetByIdAsync(maVatTu);
        if (vt is null) return null;

        return new VatTuFormViewModel
        {
            MaVatTu = vt.MaVatTu,
            TenVatTu = vt.TenVatTu,
            DonViTinh = vt.DonViTinh,
            DonGiaMacDinh = vt.DonGiaMacDinh,
            SoLuongTon = vt.SoLuongTon,
            TrangThai = vt.TrangThai
        };
    }

    public async Task<ServiceResult<int>> ThemAsync(VatTuFormViewModel vm)
    {
        var tenChuan = vm.TenVatTu.Trim();
        if (await _repo.TenDaTonTaiAsync(tenChuan))
            return ServiceResult<int>.Fail($"Tên vật tư \"{tenChuan}\" đã tồn tại trong danh mục.", nameof(vm.TenVatTu));

        if (vm.DonGiaMacDinh < 0)
            return ServiceResult<int>.Fail("Đơn giá mặc định không được âm.", nameof(vm.DonGiaMacDinh));

        if (vm.SoLuongTon < 0)
            return ServiceResult<int>.Fail("Số lượng tồn kho ban đầu không được âm.", nameof(vm.SoLuongTon));

        var vatTu = new VatTu
        {
            TenVatTu = tenChuan,
            DonViTinh = vm.DonViTinh.Trim(),
            DonGiaMacDinh = decimal.Round(vm.DonGiaMacDinh, 0),
            SoLuongTon = vm.SoLuongTon,
            TrangThai = vm.TrangThai
        };

        await _repo.AddAsync(vatTu);
        await _repo.SaveChangesAsync();

        return ServiceResult<int>.Ok(vatTu.MaVatTu);
    }

    public async Task<ServiceResult> SuaAsync(VatTuFormViewModel vm)
    {
        var vt = await _repo.LayTheoMaTrackingAsync(vm.MaVatTu);
        if (vt is null)
            return ServiceResult.Fail("Không tìm thấy thông tin vật tư cần cập nhật.");

        var tenChuan = vm.TenVatTu.Trim();
        if (await _repo.TenDaTonTaiAsync(tenChuan, vm.MaVatTu))
            return ServiceResult.Fail($"Tên vật tư \"{tenChuan}\" đã trùng với vật tư khác trong danh mục.", nameof(vm.TenVatTu));

        if (vm.DonGiaMacDinh < 0)
            return ServiceResult.Fail("Đơn giá mặc định không được âm.", nameof(vm.DonGiaMacDinh));

        if (vm.SoLuongTon < 0)
            return ServiceResult.Fail("Số lượng tồn kho không được âm.", nameof(vm.SoLuongTon));

        vt.TenVatTu = tenChuan;
        vt.DonViTinh = vm.DonViTinh.Trim();
        vt.DonGiaMacDinh = decimal.Round(vm.DonGiaMacDinh, 0);
        vt.SoLuongTon = vm.SoLuongTon;
        vt.TrangThai = vm.TrangThai;

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> XoaAsync(int maVatTu)
    {
        var vt = await _repo.GetByIdAsync(maVatTu);
        if (vt is null)
            return ServiceResult.Fail("Vật tư không tồn tại hoặc đã bị xóa trước đó.");

        // Kiểm tra an toàn: Nếu vật tư đã từng được xuất dùng trong ChiTietXuLy -> Không cho xóa vật lý
        var soLanSuDung = await _repo.DemSoLanSuDungAsync(maVatTu);
        if (soLanSuDung > 0)
        {
            return ServiceResult.Fail(
                $"Vật tư \"{vt.TenVatTu}\" đã được ghi nhận trong {soLanSuDung} lượt sửa chữa/bảo trì, không thể xóa vĩnh viễn. " +
                "Vui lòng chuyển trạng thái sang \"Ngừng sử dụng\" để bảo toàn lịch sử dữ liệu."
            );
        }

        _repo.Remove(vt);
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChuyenTrangThaiAsync(int maVatTu)
    {
        var vt = await _repo.LayTheoMaTrackingAsync(maVatTu);
        if (vt is null)
            return ServiceResult.Fail("Không tìm thấy vật tư.");

        vt.TrangThai = !vt.TrangThai;
        await _repo.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<byte[]> XuatExcelAsync(VatTuFilterViewModel filter)
    {
        var danhSach = await _repo.LayTatCaDeXuatExcelAsync(filter);

        var exportData = danhSach.Select(v => new VatTuExportRow(
            MaVatTuText: $"VT-{v.MaVatTu:D4}",
            TenVatTu: v.TenVatTu,
            DonViTinh: v.DonViTinh,
            DonGiaMacDinh: v.DonGiaMacDinh,
            SoLuongTon: v.SoLuongTon,
            TongGiaTri: v.SoLuongTon * v.DonGiaMacDinh,
            TinhTrangTon: v.SoLuongTon <= 0 ? "Hết hàng" : (v.SoLuongTon <= 5 ? "Sắp hết hàng" : "Còn hàng"),
            TrangThaiText: v.TrangThai ? "Đang sử dụng" : "Ngừng sử dụng"
        )).ToList();

        var columns = new List<ExcelColumn<VatTuExportRow>>
        {
            new("Mã VT", d => d.MaVatTuText, alignment: XLAlignmentHorizontalValues.Center, width: 12),
            new("Tên vật tư / Linh kiện", d => d.TenVatTu, alignment: XLAlignmentHorizontalValues.Left, width: 35),
            new("ĐVT", d => d.DonViTinh, alignment: XLAlignmentHorizontalValues.Center, width: 12),
            new("Đơn giá mặc định", d => d.DonGiaMacDinh, numberFormat: "#,##0 ₫", alignment: XLAlignmentHorizontalValues.Right, width: 18),
            new("Tồn kho", d => d.SoLuongTon, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 14),
            new("Tổng giá trị tồn", d => d.TongGiaTri, numberFormat: "#,##0 ₫", alignment: XLAlignmentHorizontalValues.Right, width: 20),
            new("Tình trạng kho", d => d.TinhTrangTon, alignment: XLAlignmentHorizontalValues.Center, width: 16),
            new("Trạng thái", d => d.TrangThaiText, alignment: XLAlignmentHorizontalValues.Center, width: 16)
        };

        return ExcelExportHelper.Export<VatTuExportRow>(
            sheetName: "KhoVatTu",
            reportTitle: "BÁO CÁO TỒN KHO VẬT TƯ & LINH KIỆN",
            data: exportData,
            columns: columns,
            nguoiXuat: "maintainPro System",
            coCotStt: true
        );
    }
}
