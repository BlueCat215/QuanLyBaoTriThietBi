using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers
{
    [CustomAuthorizationFilter]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
