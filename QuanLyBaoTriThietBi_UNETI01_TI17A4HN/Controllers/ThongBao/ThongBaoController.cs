// Họ tên: Phạm Việt Hoàn - Nguyễn Khắc Hiếu
// MSSV: 23103100207 - 23103100225
// Nội dung: Module 3 & 5 - STT 59: Controller Thông báo nội bộ (In-App Notification Bell & AJAX)
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

    /// AJAX: Lấy số lượng thông báo chưa đọc để cập nhật badge đỏ trên chuông
    [HttpGet]
    public async Task<IActionResult> SoChuaDoc()
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return Json(new { count = 0 });

        var vm = await _service.LayThongBaoNhanhAsync(maTk);
        return Json(new { count = vm.SoChuaDoc });
    }

    /// AJAX: Lấy danh sách thông báo nhanh khi người dùng click mở dropdown chuông
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

    /// AJAX: Đánh dấu một thông báo là đã đọc
    [HttpPost]
    public async Task<IActionResult> DanhDauDaDoc(int id)
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return Json(new { success = false, message = "Chưa đăng nhập" });

        var ok = await _service.DanhDauDaDocAsync(id, maTk);
        return Json(new { success = ok });
    }

    /// AJAX: Đánh dấu tất cả thông báo của tài khoản là đã đọc
    [HttpPost]
    public async Task<IActionResult> DanhDauTatCaDaDoc()
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return Json(new { success = false, message = "Chưa đăng nhập" });

        var count = await _service.DanhDauTatCaDaDocAsync(maTk);
        return Json(new { success = true, count });
    }

    /// Trang danh sách đầy đủ toàn bộ thông báo (/ThongBao/Index)
    [HttpGet]
    public async Task<IActionResult> Index(bool? chiChuaDoc, int trang = 1)
    {
        if (MaTaiKhoanHienTai is not { } maTk)
            return RedirectToAction("DangNhap", "Auth");

        var vm = await _service.LayDanhSachAsync(maTk, chiChuaDoc, trang);
        return View(vm);
    }
}
