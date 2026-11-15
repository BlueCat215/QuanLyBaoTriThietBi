// Họ tên: Nguyễn Khắc Hiếu - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 5: Dashboard Tổng quan - DashboardController.cs
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class DashboardController : Controller
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    // GET: /Dashboard/Index
    [HttpGet]
    public async Task<IActionResult> Index(int? thang, int? nam)
    {
        var vm = await _service.LayThongKeAsync(thang, nam);
        return View(vm);
    }
}
