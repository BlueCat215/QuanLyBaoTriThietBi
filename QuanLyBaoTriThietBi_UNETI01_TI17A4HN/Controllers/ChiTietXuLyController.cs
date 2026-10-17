// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Controller CRUD Chi tiết xử lý (chỉ nhận request, gọi Service, trả View)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

// TODO tuần 3 (phân quyền, việc 23): thêm [Authorize(Roles = "Admin,KyThuatVien")];
// KTV chỉ được ghi chi tiết của phiếu mình đang được phân công (cùng việc 39 - Bắt đầu/cập nhật xử lý).
public class ChiTietXuLyController : Controller
{
    private readonly IChiTietXuLyService _service;

    public ChiTietXuLyController(IChiTietXuLyService service) => _service = service;

    /// Danh sách phiếu xử lý để chọn phiếu cần xem/nhập chi tiết.
    public async Task<IActionResult> Index(TrangThaiPhieuXuLyEnum? trangThai) =>
        View(await _service.LayDanhSachPhieuAsync(trangThai));

    /// Chi tiết xử lý của 1 phiếu + tổng chi phí. Các Module khác (PhieuXuLy, Lịch sử thiết bị) có thể link vào đây.
    public async Task<IActionResult> TheoPhieu(int maPhieu)
    {
        var vm = await _service.LayTheoPhieuAsync(maPhieu);
        if (vm is null)
            return NotFound();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int maPhieu)
    {
        var vm = await _service.LayDeThemAsync(maPhieu);
        if (vm?.Phieu is null)
            return NotFound();

        if (!vm.Phieu.ChoPhepGhiChiTiet)
        {
            TempData["Loi"] = "Chỉ được thêm chi tiết xử lý khi phiếu đang ở trạng thái Đang xử lý.";
            return RedirectToAction(nameof(TheoPhieu), new { maPhieu });
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChiTietXuLyFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _service.ThemAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã thêm chi tiết xử lý.";
                return RedirectToAction(nameof(TheoPhieu), new { maPhieu = vm.MaPhieuXuLy });
            }
            ModelState.AddServiceErrors(result);
        }

        await _service.NapDanhMucAsync(vm);
        if (vm.Phieu is null)
            return NotFound();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _service.LayDeSuaAsync(id);
        if (vm?.Phieu is null)
            return NotFound();

        if (!vm.Phieu.ChoPhepGhiChiTiet)
        {
            TempData["Loi"] = "Phiếu không còn ở trạng thái Đang xử lý nên không thể sửa chi tiết xử lý.";
            return RedirectToAction(nameof(TheoPhieu), new { maPhieu = vm.MaPhieuXuLy });
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ChiTietXuLyFormViewModel vm)
    {
        if (id != vm.MaChiTiet)
            return NotFound();

        if (ModelState.IsValid)
        {
            var result = await _service.SuaAsync(vm);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã cập nhật chi tiết xử lý.";
                return RedirectToAction(nameof(TheoPhieu), new { maPhieu = vm.MaPhieuXuLy });
            }
            ModelState.AddServiceErrors(result);
        }

        // Service đã đặt lại vm.MaPhieuXuLy theo DB khi sửa; nạp lại Phieu + vật tư để hiển thị form.
        var goc = await _service.LayDeSuaAsync(id);
        if (goc is null)
            return NotFound();
        vm.MaPhieuXuLy = goc.MaPhieuXuLy;
        await _service.NapDanhMucAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int maPhieu)
    {
        var result = await _service.XoaAsync(id);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = "Đã xóa dòng chi tiết xử lý.";
            return RedirectToAction(nameof(TheoPhieu), new { maPhieu = result.Data });
        }

        TempData["Loi"] = result.ErrorMessage;
        return RedirectToAction(nameof(TheoPhieu), new { maPhieu });
    }
}
