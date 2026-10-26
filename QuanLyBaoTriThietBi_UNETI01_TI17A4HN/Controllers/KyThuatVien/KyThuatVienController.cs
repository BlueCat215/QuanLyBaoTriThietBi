// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Controller CRUD Kỹ thuật viên (chỉ nhận request, gọi Service, trả View)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[CustomAuthorizationFilter(Roles = "Admin")]
public class KyThuatVienController : Controller
{
    private readonly IKyThuatVienService _service;

    public KyThuatVienController(IKyThuatVienService service) => _service = service;

    public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai) =>
        View(await _service.LayDanhSachAsync(tuKhoa, trangThai));

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new KyThuatVienFormViewModel();
        await NapTaiKhoanAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KyThuatVienFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _service.ThemAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã thêm kỹ thuật viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddServiceErrors(result);
        }

        await NapTaiKhoanAsync(vm);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _service.LayDeSuaAsync(id);
        if (vm is null)
            return NotFound();

        await NapTaiKhoanAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KyThuatVienFormViewModel vm)
    {
        if (id != vm.MaKyThuatVien)
            return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _service.SuaAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã cập nhật kỹ thuật viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddServiceErrors(result);
        }

        await NapTaiKhoanAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var result = await _service.DoiTrangThaiAsync(id);
        if (!result.Succeeded)
            TempData["Loi"] = result.ErrorMessage;
        else if (result.Data!.HoatDong)
            TempData["ThanhCong"] = "Đã kích hoạt lại kỹ thuật viên.";
        else
        {
            var msg = "Đã chuyển kỹ thuật viên sang ngừng hoạt động (không nhận phân công mới).";
            if (result.Data.SoPhanCongDangHieuLuc > 0)
                msg += $" Còn {result.Data.SoPhanCongDangHieuLuc} phân công đang hiệu lực, hãy đổi kỹ thuật viên cho các phiếu này.";
            TempData["ThanhCong"] = msg;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.XoaAsync(id);
        if (result.Succeeded)
            TempData["ThanhCong"] = "Đã xóa kỹ thuật viên.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private async Task NapTaiKhoanAsync(KyThuatVienFormViewModel vm)
    {
        var ds = await _service.LayTaiKhoanChoChonAsync(vm.MaTaiKhoan);
        vm.TaiKhoanOptions = ds.Select(x => new SelectListItem(x.Ten, x.Ma.ToString())).ToList();
    }
}
