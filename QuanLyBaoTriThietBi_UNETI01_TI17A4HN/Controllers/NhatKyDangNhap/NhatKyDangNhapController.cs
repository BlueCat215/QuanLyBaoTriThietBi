// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Controller tra cứu Nhật ký đăng nhập (Audit Log - bảo vệ theo Role Admin)
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class NhatKyDangNhapController : Controller
{
    private readonly INhatKyDangNhapService _service;

    public NhatKyDangNhapController(INhatKyDangNhapService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] NhatKyDangNhapBoLoc boLoc)
    {
        var vm = await _service.LayDanhSachAsync(boLoc);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> XuatExcel([FromQuery] NhatKyDangNhapBoLoc boLoc)
    {
        var nguoiXuat = User.FindFirst("HoTen")?.Value ?? User.Identity?.Name ?? "Quản trị viên";
        var bytes = await _service.XuatExcelAsync(boLoc, nguoiXuat);
        var fileName = $"NhatKyDangNhap_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, ExcelExportHelper.ContentType, fileName);
    }
}
