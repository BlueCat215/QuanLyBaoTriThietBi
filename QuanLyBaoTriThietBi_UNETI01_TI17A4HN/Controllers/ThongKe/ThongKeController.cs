// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Controller Báo cáo thống kê (SST 45: mục 9.8 - Phân quyền Admin, thống kê đa chiều theo LINQ)
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class ThongKeController : Controller
{
    private readonly IThongKeService _service;

    public ThongKeController(IThongKeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Index(ThongKeBoLoc boLoc)
    {
        if (boLoc.KhoangNgayKhongHopLe)
        {
            TempData["Loi"] = "Khoảng ngày không hợp lệ: 'Từ ngày' không thể lớn hơn 'Đến ngày'.";
            boLoc.TuNgay = null;
            boLoc.DenNgay = null;
        }

        var vm = await _service.LayThongKeAsync(boLoc);
        return View(vm);
    }
}
