// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Service Nhật ký đăng nhập (tích hợp ClosedXML ExcelExportHelper)
#nullable enable
using ClosedXML.Excel;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class NhatKyDangNhapService : INhatKyDangNhapService
{
    private readonly INhatKyDangNhapRepository _repo;

    public NhatKyDangNhapService(INhatKyDangNhapRepository repo)
    {
        _repo = repo;
    }

    public async Task<NhatKyDangNhapIndexViewModel> LayDanhSachAsync(NhatKyDangNhapBoLoc boLoc)
    {
        var pagedItems = await _repo.LayDanhSachPagedAsync(boLoc);
        var thongKe = await _repo.LayThongKeAsync();

        return new NhatKyDangNhapIndexViewModel
        {
            BoLoc = boLoc,
            PagedItems = pagedItems,
            ThongKe = thongKe
        };
    }

    public async Task<byte[]> XuatExcelAsync(NhatKyDangNhapBoLoc boLoc, string nguoiXuat)
    {
        var danhSach = await _repo.LayTatCaChoXuatExcelAsync(boLoc);

        var columns = new List<ExcelColumn<NhatKyDangNhapRow>>
        {
            new("Thời gian", x => x.ThoiGian, numberFormat: "dd/MM/yyyy HH:mm:ss", alignment: XLAlignmentHorizontalValues.Center),
            new("Tên đăng nhập đã gõ", x => x.TenDangNhapNhap, alignment: XLAlignmentHorizontalValues.Left),
            new("Họ tên tài khoản", x => x.HoTen ?? "—", alignment: XLAlignmentHorizontalValues.Left),
            new("Vai trò", x => x.VaiTro.HasValue ? UiHelper.Badge(x.VaiTro.Value).Text : "—", alignment: XLAlignmentHorizontalValues.Center),
            new("Địa chỉ IP", x => x.DiaChiIP ?? "—", alignment: XLAlignmentHorizontalValues.Center),
            new("Kết quả đăng nhập", x => UiHelper.Badge(x.KetQua).Text, alignment: XLAlignmentHorizontalValues.Center)
        };

        return ExcelExportHelper.Export(
            sheetName: "NhatKyDangNhap",
            reportTitle: "BÁO CÁO NHẬT KÝ ĐĂNG NHẬP HỆ THỐNG (AUDIT LOG)",
            data: danhSach,
            columns: columns,
            nguoiXuat: nguoiXuat,
            coCotStt: true
        );
    }
}
