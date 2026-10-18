// Controller CRUD Thiết bị + tìm kiếm (chỉ nhận request, gọi Service, trả View)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

// (Người sử dụng chỉ thấy thiết bị thuộc TaiKhoan.MaKhuVuc của mình).
public class ThietBiController : Controller
{
    private readonly IThietBiService _service;

    public ThietBiController(IThietBiService service) => _service = service;

    public async Task<IActionResult> Index(ThietBiBoLoc boLoc)
    {
        var vm = await _service.LayDanhSachAsync(boLoc);

        // Giá trị query string sai kiểu (ví dụ ngày không đọc được) bị bỏ qua -> báo cho người dùng biết
        if (!ModelState.IsValid)
            vm.CanhBao ??= "Một số điều kiện lọc không hợp lệ nên đã được bỏ qua.";

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await _service.LayChiTietAsync(id);
        if (vm is null)
            return NotFound();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ThietBiFormViewModel();
        await NapDanhMucAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThietBiFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _service.ThemAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã thêm thiết bị thành công.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddServiceErrors(result);
        }

        await NapDanhMucAsync(vm);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _service.LayDeSuaAsync(id);
        if (vm is null)
            return NotFound();

        await NapDanhMucAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ThietBiFormViewModel vm)
    {
        if (id != vm.MaThietBi)
            return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _service.SuaAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã cập nhật thiết bị thành công.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ModelState.AddServiceErrors(result);
        }

        await NapDanhMucAsync(vm);
        return View(vm);
    }

    /// <param name="returnUrl">Địa chỉ nội bộ để quay về sau khi đổi (danh sách giữ nguyên Tìm kiếm/Lọc/Sắp xếp, hoặc trang chi tiết).</param>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTinhTrang(
        int id,
        TinhTrangThietBiEnum tinhTrang,
        string? returnUrl
    )
    {
        var result = await _service.DoiTinhTrangAsync(id, tinhTrang);
        if (result.Succeeded)
            TempData["ThanhCong"] =
                $"Đã chuyển tình trạng thiết bị sang \"{UiHelper.Badge(result.Data).Text}\".";
        else
            TempData["Loi"] = result.ErrorMessage;

        // Chỉ chuyển hướng về địa chỉ nội bộ (chống open redirect)
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.XoaAsync(id);
        if (!result.Succeeded)
        {
            TempData["Loi"] = result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["ThanhCong"] = "Đã xóa thiết bị.";
        return RedirectToAction(nameof(Index));
    }

    private async Task NapDanhMucAsync(ThietBiFormViewModel vm)
    {
        var loai = await _service.LayLoaiChoChonAsync(vm.MaLoaiThietBi);
        var khuVuc = await _service.LayKhuVucChoChonAsync(vm.MaKhuVuc);

        vm.LoaiOptions = loai.Select(x => new SelectListItem(x.Ten, x.Ma.ToString())).ToList();
        vm.KhuVucOptions = khuVuc.Select(x => new SelectListItem(x.Ten, x.Ma.ToString())).ToList();
    }
}
