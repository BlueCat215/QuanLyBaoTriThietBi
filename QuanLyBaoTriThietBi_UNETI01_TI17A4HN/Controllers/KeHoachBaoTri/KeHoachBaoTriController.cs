// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Controller CRUD & Vòng đời trạng thái Kế hoạch bảo trì (Workflow: Chờ thực hiện -> Đã phân công -> Đang thực hiện -> Chờ nghiệm thu -> Hoàn thành/Đã hủy)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
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
    public async Task<IActionResult> Details(int id)
    {
        var result = await _service.LayChiTietAsync(id);
        if (!result.Succeeded)
        {
            TempData["Loi"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(int? maThietBi)
    {
        var vm = new KeHoachBaoTriFormViewModel { MaThietBi = maThietBi };
        await NapThietBiAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(KeHoachBaoTriFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _service.ThemAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã thêm kế hoạch bảo trì thành công.";
                return RedirectToAction(nameof(Details), new { id = result.Data });
            }
            ModelState.AddServiceErrors(result);
        }

        await NapThietBiAsync(vm);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
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
    [Authorize(Roles = AppRoles.Admin)]
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
                return RedirectToAction(nameof(Details), new { id });
            }
            ModelState.AddServiceErrors(result);
        }

        await NapThietBiAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Huy(int id)
    {
        var result = await _service.HuyAsync(id);
        if (result.Succeeded)
            TempData["ThanhCong"] = "Đã hủy kế hoạch bảo trì.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Chuyển trạng thái kế hoạch theo vòng đời (STT 31)
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChuyenTrangThai(
        int id,
        TrangThaiKeHoachEnum trangThaiMoi,
        string? ghiChu,
        string? returnUrl
    )
    {
        var result = await _service.ChuyenTrangThaiAsync(id, trangThaiMoi, ghiChu);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = $"Đã chuyển trạng thái kế hoạch sang '{KeHoachWorkflowHelper.GetDisplayName(trangThaiMoi)}'.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task NapThietBiAsync(KeHoachBaoTriFormViewModel vm)
    {
        var ds = await _service.LayThietBiChoChonAsync(vm.MaThietBi);
        vm.ThietBiOptions = ds.Select(x => new SelectListItem(x.Ten, x.Ma.ToString())).ToList();
    }
}
