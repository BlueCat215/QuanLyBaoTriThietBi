// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Controller CRUD Loại thiết bị (chỉ nhận request, gọi Service, trả View)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

// TODO tuần 3 (phân quyền): thêm [Authorize(Roles = "Admin")] khi đã cấu hình đăng nhập.
public class LoaiThietBiController : Controller
{
    private const string FieldsBind = "MaLoaiThietBi,TenLoaiThietBi,ChuKyBaoTriMacDinhThang,MoTa,TrangThai";

    private readonly ILoaiThietBiService _service;

    public LoaiThietBiController(ILoaiThietBiService service) => _service = service;

    public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai)
        => View(await _service.LayDanhSachAsync(tuKhoa, trangThai));

    [HttpGet]
    public IActionResult Create() => View(new LoaiThietBi { ChuKyBaoTriMacDinhThang = 12, TrangThai = true });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(FieldsBind)] LoaiThietBi model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _service.ThemAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View(model);
        }

        TempData["ThanhCong"] = "Đã thêm loại thiết bị thành công.";
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
    public async Task<IActionResult> Edit(int id, [Bind(FieldsBind)] LoaiThietBi model)
    {
        if (id != model.MaLoaiThietBi) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var result = await _service.SuaAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View(model);
        }

        TempData["ThanhCong"] = "Đã cập nhật loại thiết bị thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var result = await _service.DoiTrangThaiAsync(id);
        if (result.Succeeded)
            TempData["ThanhCong"] = result.Data ? "Đã kích hoạt lại loại thiết bị." : "Đã chuyển loại thiết bị sang Ngừng sử dụng.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.XoaAsync(id);
        if (result.Succeeded)
            TempData["ThanhCong"] = "Đã xóa loại thiết bị.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }
}
