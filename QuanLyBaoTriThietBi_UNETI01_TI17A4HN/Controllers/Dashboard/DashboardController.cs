// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Controller Dashboard (chỉ nhận request, gọi Service, trả View)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

// TODO tuần 3 (phân quyền, việc 23): thêm [Authorize(Roles = "Admin")] (Dashboard chỉ hiện trong menu Admin).
public class DashboardController : Controller
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    public async Task<IActionResult> Index() => View(await _service.LayThongKeAsync());
}
