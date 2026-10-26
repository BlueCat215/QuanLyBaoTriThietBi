// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Controller CRUD Kế hoạch bảo trì (chỉ nhận request, gọi Service, trả View)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[CustomAuthorizationFilter(Roles = "Admin,KyThuatVien")]
public class KeHoachBaoTriController : Controller
{
    private readonly IKeHoachBaoTriService _service;

    public KeHoachBaoTriController(IKeHoachBaoTriService service) => _service = service;

    public async Task<IActionResult> Index(
        string? tuKhoa,
        TrangThaiKeHoachEnum? trangThai,
        MucDoUuTienEnum? mucDo
    ) => View(await _service.LayDanhSachAsync(tuKhoa, trangThai, mucDo));

    [HttpGet]
    [CustomAuthorizationFilter(Roles = "Admin")]
    public async Task<IActionResult> Create(int? maThietBi)
    {
        var vm = new KeHoachBaoTriFormViewModel { MaThietBi = maThietBi };
        await NapThietBiAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [CustomAuthorizationFilter(Roles = "Admin")]
    public async Task<IActionResult> Create(KeHoachBaoTriFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _service.ThemAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã thêm kế hoạch bảo trì thành công.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddServiceErrors(result);
        }

        await NapThietBiAsync(vm);
        return View(vm);
    }

    [HttpGet]
    [CustomAuthorizationFilter(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _service.LayDeSuaAsync(id);
        if (!result.Succeeded)
        {
            TempData["Loi"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        await NapThietBiAsync(result.Data!);
        return View(result.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [CustomAuthorizationFilter(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, KeHoachBaoTriFormViewModel vm)
    {
        if (id != vm.MaKeHoach)
            return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _service.SuaAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã cập nhật kế hoạch bảo trì thành công.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddServiceErrors(result);
        }

        await NapThietBiAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var result = await _service.HuyAsync(id);
        if (result.Succeeded)
            TempData["ThanhCong"] = "Đã hủy kế hoạch bảo trì.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private async Task NapThietBiAsync(KeHoachBaoTriFormViewModel vm)
    {
        var ds = await _service.LayThietBiChoChonAsync(vm.MaThietBi);
        vm.ThietBiOptions = ds.Select(x => new SelectListItem(x.Ten, x.Ma.ToString())).ToList();
    }
}
