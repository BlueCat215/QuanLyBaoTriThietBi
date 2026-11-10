using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Trang điều hướng trung tâm: tự động chuyển người dùng về trang chức năng phù hợp theo vai trò
        /// </summary>
        public IActionResult Index()
        {
            if (User.IsInRole(AppRoles.Admin))
                return RedirectToAction("Index", "Dashboard");

            if (User.IsInRole(AppRoles.KyThuatVien))
                return RedirectToAction("ViecCuaToi", "PhieuXuLy");

            if (User.IsInRole(AppRoles.NguoiSuDung))
                return RedirectToAction("Index", "ThietBi");

            return RedirectToAction("DangNhap", "Auth");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
