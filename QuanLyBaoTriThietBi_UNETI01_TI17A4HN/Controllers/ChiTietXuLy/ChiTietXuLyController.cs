// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Controller CRUD Chi tiết xử lý và Bắt đầu xử lý (STT 39: chỉ KTV đang được phân công hoặc Admin)
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

// KTV chỉ được ghi chi tiết của phiếu mình đang được phân công (hoặc Admin)
[Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
public class ChiTietXuLyController : Controller
{
    private readonly IChiTietXuLyService _service;

    public ChiTietXuLyController(IChiTietXuLyService service) => _service = service;

    private bool LaAdmin => User.IsInRole(nameof(VaiTroEnum.Admin));
    private int? MaTaiKhoanDangNhap => int.TryParse(User.FindFirst("MaTaiKhoan")?.Value, out var id) ? id : null;

    /// Danh sách phiếu xử lý để chọn phiếu cần xem/nhập chi tiết.
    public async Task<IActionResult> Index(TrangThaiPhieuXuLyEnum? trangThai) =>
        View(await _service.LayDanhSachPhieuAsync(trangThai));

    /// Chi tiết xử lý của 1 phiếu + tổng chi phí.
    public async Task<IActionResult> TheoPhieu(int maPhieu)
    {
        var vm = await _service.LayTheoPhieuAsync(maPhieu, MaTaiKhoanDangNhap, LaAdmin);
        if (vm is null)
            return NotFound();
        return View(vm);
    }

    /// <summary>
    /// STT 39: Bắt đầu xử lý phiếu (chỉ KTV đang được phân công hoặc Admin)
    /// Ghi nhận NgayBatDau, chuyển phiếu sang Đang xử lý
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BatDauXuLy(int maPhieuXuLy)
    {
        var result = await _service.BatDauXuLyAsync(maPhieuXuLy, MaTaiKhoanDangNhap, LaAdmin);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = $"Đã bắt đầu xử lý phiếu {UiHelper.Ma("PX", maPhieuXuLy)}. Phiếu đã chuyển sang trạng thái Đang xử lý.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }
        return RedirectToAction(nameof(TheoPhieu), new { maPhieu = maPhieuXuLy });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int maPhieu)
    {
        var vm = await _service.LayDeThemAsync(maPhieu, MaTaiKhoanDangNhap, LaAdmin);
        if (vm?.Phieu is null)
            return NotFound();

        if (!vm.LaAdmin && !vm.LaKtvPhuTrach)
        {
            TempData["Loi"] = "Bạn không phải là kỹ thuật viên đang được phân công xử lý phiếu này.";
            return RedirectToAction(nameof(TheoPhieu), new { maPhieu });
        }

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
            var result = await _service.ThemAsync(vm, MaTaiKhoanDangNhap, LaAdmin);
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
        var vm = await _service.LayDeSuaAsync(id, MaTaiKhoanDangNhap, LaAdmin);
        if (vm?.Phieu is null)
            return NotFound();

        if (!vm.LaAdmin && !vm.LaKtvPhuTrach)
        {
            TempData["Loi"] = "Bạn không phải là kỹ thuật viên đang được phân công xử lý phiếu này.";
            return RedirectToAction(nameof(TheoPhieu), new { maPhieu = vm.MaPhieuXuLy });
        }

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
            var result = await _service.SuaAsync(vm, MaTaiKhoanDangNhap, LaAdmin);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã cập nhật chi tiết xử lý.";
                return RedirectToAction(nameof(TheoPhieu), new { maPhieu = vm.MaPhieuXuLy });
            }
            ModelState.AddServiceErrors(result);
        }

        // Service đã đặt lại vm.MaPhieuXuLy theo DB khi sửa; nạp lại Phieu + vật tư để hiển thị form.
        var goc = await _service.LayDeSuaAsync(id, MaTaiKhoanDangNhap, LaAdmin);
        if (goc is null)
            return NotFound();
        vm.MaPhieuXuLy = goc.MaPhieuXuLy;
        await _service.NapDanhMucAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int maPhieu)
    {
        var result = await _service.XoaAsync(id, MaTaiKhoanDangNhap, LaAdmin);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = "Đã xóa dòng chi tiết xử lý.";
            return RedirectToAction(nameof(TheoPhieu), new { maPhieu = result.Data });
        }

        TempData["Loi"] = result.ErrorMessage;
        return RedirectToAction(nameof(TheoPhieu), new { maPhieu });
    }
}
