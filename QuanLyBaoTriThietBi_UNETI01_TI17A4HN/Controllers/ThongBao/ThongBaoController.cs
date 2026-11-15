// Họ tên: Phạm Việt Hoàn - Nguyễn Khắc Hiếu - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 3 & 5: Hệ thống Thông báo nội bộ - ThongBaoController.cs
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize]
public class ThongBaoController : Controller
{
    private readonly IThongBaoService _service;

    public ThongBaoController(IThongBaoService service) => _service = service;

    private int? MaTaiKhoanHienTai
    {
        get
        {
            if (int.TryParse(User.FindFirst("MaTaiKhoan")?.Value, out var id))
                return id;
            return HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan);
        }
    }

    // GET: /ThongBao/SoChuaDoc (AJAX)
    [HttpGet]
    public async Task<IActionResult> SoChuaDoc()
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return Json(new { count = 0 });

        var vm = await _service.LayThongBaoNhanhAsync(maTk);
        return Json(new { count = vm.SoChuaDoc });
    }

    // GET: /ThongBao/DanhSachNhanh (AJAX)
    [HttpGet]
    public async Task<IActionResult> DanhSachNhanh()
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return Json(new { soChuaDoc = 0, items = Array.Empty<object>() });

        var vm = await _service.LayThongBaoNhanhAsync(maTk);
        return Json(new
        {
            soChuaDoc = vm.SoChuaDoc,
            items = vm.DanhSach
        });
    }

    // POST: /ThongBao/DanhDauDaDoc (AJAX)
    [HttpPost]
    public async Task<IActionResult> DanhDauDaDoc(int id)
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return Json(new { success = false, message = "Chưa đăng nhập" });

        var ok = await _service.DanhDauDaDocAsync(id, maTk);
        return Json(new { success = ok });
    }

    // POST: /ThongBao/DanhDauTatCaDaDoc (AJAX)
    [HttpPost]
    public async Task<IActionResult> DanhDauTatCaDaDoc()
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return Json(new { success = false, message = "Chưa đăng nhập" });

        var count = await _service.DanhDauTatCaDaDocAsync(maTk);
        return Json(new { success = true, count });
    }

    // GET: /ThongBao/Index
    [HttpGet]
    public async Task<IActionResult> Index(bool? chiChuaDoc, int trang = 1)
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return RedirectToAction("DangNhap", "Auth");

        var vm = await _service.LayDanhSachAsync(maTk, chiChuaDoc, trang);
        return View(vm);
    }
}
