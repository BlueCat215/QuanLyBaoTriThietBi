// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Controller Đăng nhập / Đăng xuất (lưu MaTaiKhoan, HoTen, VaiTro vào Session)
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpGet]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new DangNhapViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _auth.DangNhapAsync(vm.TenDangNhap, vm.MatKhau, ip);

        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            vm.MatKhau = string.Empty;
            return View(vm);
        }

        var phien = result.Data!;

        // Xóa dữ liệu phiên cũ (nếu có) rồi ghi thông tin người dùng mới
        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32(SessionKeys.MaTaiKhoan, phien.MaTaiKhoan);
        HttpContext.Session.SetString(SessionKeys.TenDangNhap, phien.TenDangNhap);
        HttpContext.Session.SetString(SessionKeys.HoTen, phien.HoTen);
        HttpContext.Session.SetString(SessionKeys.VaiTro, phien.VaiTro.ToString());

        // Chỉ chuyển hướng về địa chỉ nội bộ (chống open redirect)
        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return LocalRedirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DangXuat()
    {
        HttpContext.Session.Clear();
        TempData["ThanhCong"] = "Bạn đã đăng xuất.";
        return RedirectToAction(nameof(DangNhap));
    }

    [HttpGet]
    public IActionResult TuChoiTruyCap()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
