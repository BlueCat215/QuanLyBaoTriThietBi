// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý Hệ thống & Danh mục dùng chung - KhuVucController.cs
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

    // GET: /KhuVuc
    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
        => View(await _service.LayDanhSachAsync(tuKhoa, trangThai));

    // GET: /KhuVuc/Create
    [HttpGet]
    public IActionResult Create() => View(new KhuVuc { TrangThai = true });

    // POST: /KhuVuc/Create
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

    // GET: /KhuVuc/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _service.LayTheoIdAsync(id);
        if (model is null) return NotFound();
        return View(model);
    }

    // POST: /KhuVuc/Edit/{id}
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

    // POST: /KhuVuc/DoiTrangThai/{id}
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

    // POST: /KhuVuc/Delete/{id}
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
