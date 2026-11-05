// Họ tên: Nguyễn Khắc Hiếu
// MSSV: 23103100225
// Nội dung: Module 5 - STT 57: Controller Quản lý danh mục Kho vật tư (CRUD, tìm kiếm, lọc cảnh báo tồn kho, xuất Excel)
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize(Roles = "Admin,KyThuatVien")]
public class VatTuController : Controller
{
    private readonly IVatTuService _service;

    public VatTuController(IVatTuService service)
    {
        _service = service;
    }

    private bool LaAdmin => User.IsInRole("Admin");

    // ==========================================
    // 1. DANH SÁCH & BỘ LỌC
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] VatTuFilterViewModel filter)
    {
        var vm = await _service.LayDanhSachAsync(filter, LaAdmin);
        return View(vm);
    }

    // ==========================================
    // 2. THÊM MỚI VẬT TƯ (Chỉ Admin)
    // ==========================================
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new VatTuFormViewModel
        {
            TrangThai = true,
            SoLuongTon = 0,
            DonGiaMacDinh = 0
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VatTuFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _service.ThemAsync(vm);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = $"Đã thêm vật tư \"{vm.TenVatTu}\" vào kho thành công.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddServiceErrors(result);
        return View(vm);
    }

    // ==========================================
    // 3. CẬP NHẬT VẬT TƯ (Chỉ Admin)
    // ==========================================
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _service.LayDeSuaAsync(id);
        if (vm is null)
        {
            TempData["Loi"] = "Không tìm thấy vật tư cần sửa.";
            return RedirectToAction(nameof(Index));
        }

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VatTuFormViewModel vm)
    {
        if (id != vm.MaVatTu)
            return NotFound();

        if (!ModelState.IsValid)
            return View(vm);

        var result = await _service.SuaAsync(vm);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = $"Đã cập nhật thông tin vật tư \"{vm.TenVatTu}\".";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddServiceErrors(result);
        return View(vm);
    }

    // ==========================================
    // 4. BẬT / TẮT TRẠNG THÁI SỬ DỤNG (Chỉ Admin)
    // ==========================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChuyenTrangThai(int id)
    {
        var result = await _service.ChuyenTrangThaiAsync(id);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = "Đã cập nhật trạng thái hoạt động của vật tư.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // 5. XÓA VẬT TƯ (Chỉ Admin - Có kiểm tra an toàn)
    // ==========================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var result = await _service.XoaAsync(id);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = "Đã xóa vật tư khỏi danh mục.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // 6. XUẤT EXCEL DANH SÁCH KHO VẬT TƯ (STT 61)
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> XuatExcel([FromQuery] VatTuFilterViewModel filter)
    {
        try
        {
            var fileBytes = await _service.XuatExcelAsync(filter);
            var fileName = $"BaoCao_KhoVatTu_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            TempData["Loi"] = $"Lỗi khi xuất file Excel: {ex.Message}";
            return RedirectToAction(nameof(Index), filter);
        }
    }
}
