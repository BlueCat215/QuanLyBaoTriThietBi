// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Controller CRUD Tài khoản (chỉ nhận request, gọi Service, trả View)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[CustomAuthorizationFilter(Roles = "Admin")]
public class TaiKhoanController : Controller
{
    private readonly ITaiKhoanService _service;

    public TaiKhoanController(ITaiKhoanService service) => _service = service;

    /// <summary>
    /// MaTaiKhoan của người đang đăng nhập, đọc từ claim "MaTaiKhoan".
    /// Việc 12 (Đăng nhập) phải thêm claim này khi tạo phiên đăng nhập; trước đó trả null nên các rule "không tự khóa/xóa mình" tạm không áp dụng.
    /// </summary>
    private int? MaTaiKhoanHienTai
        => int.TryParse(User.FindFirst("MaTaiKhoan")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Index(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai)
        => View(await _service.LayDanhSachAsync(tuKhoa, vaiTro, trangThai, MaTaiKhoanHienTai));

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new TaiKhoanFormViewModel();
        await NapKhuVucAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaiKhoanFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _service.ThemAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã thêm tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddServiceErrors(result);
        }

        await NapKhuVucAsync(vm);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _service.LayDeSuaAsync(id);
        if (vm is null) return NotFound();

        await NapKhuVucAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaiKhoanFormViewModel vm)
    {
        if (id != vm.MaTaiKhoan) return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _service.SuaAsync(vm, MaTaiKhoanHienTai);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã cập nhật tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddServiceErrors(result);
        }

        await NapKhuVucAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var result = await _service.DoiTrangThaiAsync(id, MaTaiKhoanHienTai);
        if (result.Succeeded)
            TempData["ThanhCong"] = result.Data ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.XoaAsync(id, MaTaiKhoanHienTai);
        if (result.Succeeded)
            TempData["ThanhCong"] = "Đã xóa tài khoản.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    private async Task NapKhuVucAsync(TaiKhoanFormViewModel vm)
    {
        var ds = await _service.LayKhuVucChoChonAsync(vm.MaKhuVuc);
        vm.KhuVucOptions = ds
            .Select(k => new SelectListItem(k.TenKhuVuc, k.MaKhuVuc.ToString()))
            .ToList();
    }
}
