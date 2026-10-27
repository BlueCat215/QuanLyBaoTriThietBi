// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Controller CRUD Khu vực (chỉ nhận request, gọi Service, trả View)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class KhuVucController : Controller
{
    private const string FieldsBind = "MaKhuVuc,TenKhuVuc,ViTri,NguoiPhuTrach,MoTa,TrangThai";

    private readonly IKhuVucService _service;

    public KhuVucController(IKhuVucService service) => _service = service;

    public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
        => View(await _service.LayDanhSachAsync(tuKhoa, trangThai));

    [HttpGet]
    public IActionResult Create() => View(new KhuVuc { TrangThai = true });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(FieldsBind)] KhuVuc model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _service.ThemAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View(model);
        }

        TempData["ThanhCong"] = "Đã thêm khu vực thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _service.LayTheoIdAsync(id);
        if (model is null) return NotFound();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(FieldsBind)] KhuVuc model)
    {
        if (id != model.MaKhuVuc) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var result = await _service.SuaAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View(model);
        }

        TempData["ThanhCong"] = "Đã cập nhật khu vực thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var result = await _service.DoiTrangThaiAsync(id);
        if (result.Succeeded)
            TempData["ThanhCong"] = result.Data ? "Đã kích hoạt lại khu vực." : "Đã chuyển khu vực sang Ngừng hoạt động.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.XoaAsync(id);
        if (result.Succeeded)
            TempData["ThanhCong"] = "Đã xóa khu vực.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }
}
