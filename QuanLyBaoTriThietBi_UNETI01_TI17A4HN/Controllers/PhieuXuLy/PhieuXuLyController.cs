// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Controller Phiếu xử lý, Phân công, Đổi KTV và kiểm soát luồng xử lý 
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize]
public class PhieuXuLyController : Controller
{
    private readonly IPhieuXuLyService _phieuService;

    public PhieuXuLyController(IPhieuXuLyService phieuService)
    {
        _phieuService = phieuService;
    }

    private bool LaAdmin => User.IsInRole(nameof(VaiTroEnum.Admin));
    private int? MaTaiKhoanDangNhap => int.TryParse(User.FindFirst("MaTaiKhoan")?.Value, out var id) ? id : null;

    // ------------------------------------------ DANH SÁCH ------------------------------------------
    // Danh sách tất cả phiếu xử lý (dành cho Admin / KTV xem chung)
    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> Index(PhieuXuLyBoLoc boLoc)
    {
        var vm = await _phieuService.LayDanhSachAsync(boLoc);
        return View(vm);
    }

    // : Danh sách công việc của KTV đang đăng nhập
    [HttpGet]
    [Authorize(Roles = AppRoles.KyThuatVien)]
    public async Task<IActionResult> ViecCuaToi(PhieuXuLyBoLoc boLoc)
    {
        if (!MaTaiKhoanDangNhap.HasValue)
        {
            TempData["Loi"] = "Phiên làm việc hết hạn hoặc không xác thực được tài khoản.";
            return RedirectToAction("Index", "Home");
        }

        var vm = await _phieuService.LayViecCuaKtvAsync(MaTaiKhoanDangNhap.Value, boLoc);
        return View(vm);
    }

    // ------------------------------------------ CHI TIẾT ------------------------------------------
    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _phieuService.LayChiTietAsync(id, MaTaiKhoanDangNhap, LaAdmin);
        if (vm == null)
            return NotFound();

        return View(vm);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(int? maBaoCao, int? maKeHoach)
    {
        var vm = await _phieuService.ChuanBiFormTaoPhieuAsync(maBaoCao, maKeHoach);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(TaoPhieuXuLyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var formPrep = await _phieuService.ChuanBiFormTaoPhieuAsync(model.MaBaoCao, model.MaKeHoach);
            model.BaoCaoOptions = formPrep.BaoCaoOptions;
            model.KeHoachOptions = formPrep.KeHoachOptions;
            model.KtvOptions = formPrep.KtvOptions;
            return View(model);
        }

        var result = await _phieuService.TaoPhieuXuLyAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Có lỗi xảy ra khi tạo phiếu xử lý.");
            var formPrep = await _phieuService.ChuanBiFormTaoPhieuAsync(model.MaBaoCao, model.MaKeHoach);
            model.BaoCaoOptions = formPrep.BaoCaoOptions;
            model.KeHoachOptions = formPrep.KeHoachOptions;
            model.KtvOptions = formPrep.KtvOptions;
            return View(model);
        }

        TempData["ThanhCong"] = $"Đã tạo thành công Phiếu xử lý {UiHelper.Ma("PX", result.Data)}.";
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    // ------------------------------------------ PHÂN CÔNG XỬ LÝ  ------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> PhanCong(PhanCongXuLyFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Loi"] = "Dữ liệu phân công không hợp lệ. Vui lòng chọn kỹ thuật viên.";
            return RedirectToAction(nameof(Details), new { id = model.MaPhieuXuLy });
        }

        var result = await _phieuService.PhanCongXuLyAsync(model);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = "Phân công kỹ thuật viên xử lý thành công.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id = model.MaPhieuXuLy });
    }

    // ------------------------------------------ ĐỔI KỸ THUẬT VIÊN  ------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DoiKyThuatVien(DoiKyThuatVienFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Loi"] = "Vui lòng chọn kỹ thuật viên mới và ghi rõ lý do đổi.";
            return RedirectToAction(nameof(Details), new { id = model.MaPhieuXuLy });
        }

        var result = await _phieuService.DoiKyThuatVienAsync(model);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = "Đổi kỹ thuật viên thành công! Lịch sử phân công cũ đã được lưu trữ an toàn.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id = model.MaPhieuXuLy });
    }

    // ------------------------------------------ BẮT ĐẦU XỬ LÝ (STT 39) ------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> BatDauXuLy(int id)
    {
        var result = await _phieuService.BatDauXuLyAsync(id, MaTaiKhoanDangNhap, LaAdmin);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = $"Đã bắt đầu xử lý phiếu {UiHelper.Ma("PX", id)}. Phiếu đã chuyển sang trạng thái Đang xử lý.";
            return RedirectToAction("TheoPhieu", "ChiTietXuLy", new { maPhieu = id });
        }

        TempData["Loi"] = result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    // ------------------------------------------ CHUYỂN CHỜ NGHIỆM THU (STT 40) ------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> ChuyenChoNghiemThu(ChuyenChoNghiemThuViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Loi"] = "Vui lòng nhập kết quả xử lý hợp lệ trước khi chuyển nghiệm thu.";
            return RedirectToAction(nameof(Details), new { id = model.MaPhieuXuLy });
        }

        var result = await _phieuService.ChuyenChoNghiemThuAsync(model, MaTaiKhoanDangNhap, LaAdmin);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = $"Đã chuyển phiếu {UiHelper.Ma("PX", model.MaPhieuXuLy)} sang trạng thái Chờ nghiệm thu.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id = model.MaPhieuXuLy });
    }

    // ------------------------------------------ NGHIỆM THU PHIẾU (STT 40) ------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> NghiemThu(NghiemThuPhieuViewModel model)
    {
        var result = await _phieuService.NghiemThuAsync(model, MaTaiKhoanDangNhap, LaAdmin);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = model.Dat
                ? $"Nghiệm thu thành công! Phiếu {UiHelper.Ma("PX", model.MaPhieuXuLy)} đã hoàn thành và thiết bị đã trở lại hoạt động bình thường."
                : $"Đã trả phiếu {UiHelper.Ma("PX", model.MaPhieuXuLy)} về trạng thái Đang xử lý để kỹ thuật viên khắc phục.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id = model.MaPhieuXuLy });
    }

    // ------------------------------------------ HỦY PHIẾU XỬ LÝ ------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> HuyPhieu(int id, string? lyDoHuy)
    {
        var result = await _phieuService.HuyPhieuXuLyAsync(id, lyDoHuy);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = $"Đã hủy phiếu xử lý {UiHelper.Ma("PX", id)}.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
