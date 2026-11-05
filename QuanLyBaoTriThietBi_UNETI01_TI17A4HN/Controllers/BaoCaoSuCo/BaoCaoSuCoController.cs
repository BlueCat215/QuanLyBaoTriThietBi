// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Controller CRUD Báo cáo sự cố (người dùng gửi + xem báo cáo của mình, Admin xem tất cả, đính kèm ảnh hiện trường STT 55)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize]
public class BaoCaoSuCoController : Controller
{
    private const bool DungTaiKhoanXemThuKhiChuaDangNhap = false;

    private readonly IBaoCaoSuCoService _service;
    private readonly ITepDinhKemService _tepDinhKemService;

    public BaoCaoSuCoController(
        IBaoCaoSuCoService service,
        ITepDinhKemService tepDinhKemService
    )
    {
        _service = service;
        _tepDinhKemService = tepDinhKemService;
    }

    // ---------- Thông tin người đang đăng nhập (claim do việc 12 tạo: MaTaiKhoan + Role) ----------
    private bool DaDangNhap => User.Identity?.IsAuthenticated ?? false;

    private bool LaAdmin =>
        User.IsInRole("Admin") || (DungTaiKhoanXemThuKhiChuaDangNhap && !DaDangNhap);

    private async Task<NguoiThaoTac?> LayNguoiThaoTacAsync()
    {
        if (int.TryParse(User.FindFirst("MaTaiKhoan")?.Value, out var id))
            return new NguoiThaoTac(id, LaAdmin);

        if (DungTaiKhoanXemThuKhiChuaDangNhap && !DaDangNhap)
            if (await _service.LayMaTaiKhoanXemThuAsync() is { } demo)
                return new NguoiThaoTac(demo, LaAdmin);

        return null;
    }

    private IActionResult YeuCauDangNhap()
    {
        TempData["Loi"] = "Bạn cần đăng nhập để sử dụng chức năng báo cáo sự cố.";
        return RedirectToAction("Index", "Home");
    }

    private IActionResult VeDanhSach() =>
        LaAdmin ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(CuaToi));

    // ---------- Danh sách ----------
    public async Task<IActionResult> Index(
        string? tuKhoa,
        TrangThaiBaoCaoEnum? trangThai,
        MucDoSuCoEnum? mucDo
    )
    {
        if (!LaAdmin)
            return RedirectToAction(nameof(CuaToi));

        return View(await _service.LayDanhSachAsync(tuKhoa, trangThai, mucDo, null));
    }

    public async Task<IActionResult> CuaToi(
        string? tuKhoa,
        TrangThaiBaoCaoEnum? trangThai,
        MucDoSuCoEnum? mucDo
    )
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        var vm = await _service.LayDanhSachAsync(tuKhoa, trangThai, mucDo, nguoi.MaTaiKhoan);
        return View(nameof(Index), vm);
    }

    // ---------- Chi tiết ----------
    public async Task<IActionResult> Details(int id)
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        var vm = await _service.LayChiTietAsync(id, nguoi);
        if (vm is null)
            return NotFound();
        return View(vm);
    }

    // ---------- Gửi báo cáo ----------
    [HttpGet]
    public async Task<IActionResult> Create(int? maThietBi)
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        var vm = new BaoCaoSuCoFormViewModel { MaThietBi = maThietBi };
        await NapThietBiAsync(vm, nguoi);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BaoCaoSuCoFormViewModel vm)
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        if (ModelState.IsValid)
        {
            var result = await _service.GuiBaoCaoAsync(vm, nguoi);
            if (result.Succeeded)
            {
                // STT 55: Tải lên các tệp ảnh hiện trường sự cố nếu có
                if (vm.TepAnhDinhKem != null && vm.TepAnhDinhKem.Any(f => f.Length > 0))
                {
                    var uploadRes = await _tepDinhKemService.UploadNhieuTepAsync(
                        vm.TepAnhDinhKem,
                        LoaiDoiTuongDinhKemEnum.BaoCaoSuCo,
                        result.Data,
                        nguoi.MaTaiKhoan,
                        chiChapNhanAnh: true
                    );

                    if (!uploadRes.Succeeded)
                    {
                        TempData["CanhBao"] = "Báo cáo sự cố đã gửi thành công nhưng một số tệp ảnh không tải lên được: " + uploadRes.ErrorMessage;
                    }
                }

                TempData["ThanhCong"] = "Đã gửi báo cáo sự cố thành công.";
                return RedirectToAction(nameof(Details), new { id = result.Data });
            }
            ModelState.AddServiceErrors(result);
        }

        await NapThietBiAsync(vm, nguoi);
        return View(vm);
    }

    // ---------- Sửa (chỉ khi còn Mới báo) ----------
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        var result = await _service.LayDeSuaAsync(id, nguoi);
        if (!result.Succeeded)
        {
            TempData["Loi"] = result.ErrorMessage;
            return VeDanhSach();
        }

        await NapThietBiAsync(result.Data!, nguoi);
        return View(result.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BaoCaoSuCoFormViewModel vm)
    {
        if (id != vm.MaBaoCao)
            return NotFound();

        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        if (ModelState.IsValid)
        {
            var result = await _service.SuaAsync(vm, nguoi);
            if (result.Succeeded)
            {
                // STT 55: Tải lên thêm các tệp ảnh hiện trường sự cố nếu có
                if (vm.TepAnhDinhKem != null && vm.TepAnhDinhKem.Any(f => f.Length > 0))
                {
                    var uploadRes = await _tepDinhKemService.UploadNhieuTepAsync(
                        vm.TepAnhDinhKem,
                        LoaiDoiTuongDinhKemEnum.BaoCaoSuCo,
                        id,
                        nguoi.MaTaiKhoan,
                        chiChapNhanAnh: true
                    );

                    if (!uploadRes.Succeeded)
                    {
                        TempData["CanhBao"] = "Báo cáo sự cố đã cập nhật nhưng một số tệp ảnh không tải lên được: " + uploadRes.ErrorMessage;
                    }
                }

                TempData["ThanhCong"] = "Đã cập nhật báo cáo sự cố.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ModelState.AddServiceErrors(result);
        }

        await NapThietBiAsync(vm, nguoi);
        return View(vm);
    }

    /// <summary>
    /// STT 55: Tải lên thêm ảnh hiện trường sự cố từ trang Chi tiết báo cáo
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAnh(int id, List<IFormFile> files)
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        var bc = await _service.LayChiTietAsync(id, nguoi);
        if (bc is null)
            return NotFound();

        if (files == null || files.Count == 0 || files.All(f => f.Length == 0))
        {
            TempData["Loi"] = "Vui lòng chọn ít nhất một tệp ảnh để tải lên.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await _tepDinhKemService.UploadNhieuTepAsync(
            files,
            LoaiDoiTuongDinhKemEnum.BaoCaoSuCo,
            id,
            nguoi.MaTaiKhoan,
            chiChapNhanAnh: true
        );

        if (result.Succeeded)
        {
            var soLuong = result.Data?.Count ?? 0;
            TempData["ThanhCong"] = $"Đã tải lên thành công {soLuong} ảnh hiện trường sự cố.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// STT 55: Xóa ảnh hiện trường sự cố
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaAnh(int id, int maTepDinhKem, string? returnUrl)
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        var bc = await _service.LayChiTietAsync(id, nguoi);
        if (bc is null)
            return NotFound();

        // Chỉ Admin hoặc người gửi báo cáo khi còn ở Mới báo mới được xóa ảnh
        if (!nguoi.LaAdmin && bc.BaoCao.TrangThai != TrangThaiBaoCaoEnum.MoiBao)
        {
            TempData["Loi"] = "Báo cáo sự cố đã được tiếp nhận hoặc xử lý, không thể xóa ảnh hiện trường.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await _tepDinhKemService.XoaTepAsync(maTepDinhKem);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = "Đã xóa ảnh hiện trường thành công.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Details), new { id });
    }

    // ---------- Hủy / Xóa ----------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        var result = await _service.HuyAsync(id, nguoi);
        if (result.Succeeded)
            TempData["ThanhCong"] = "Đã hủy báo cáo sự cố.";
        else
            TempData["Loi"] = result.ErrorMessage;

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var nguoi = await LayNguoiThaoTacAsync();
        if (nguoi is null)
            return YeuCauDangNhap();

        var result = await _service.XoaAsync(id, nguoi);
        if (!result.Succeeded)
        {
            TempData["Loi"] = result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["ThanhCong"] = "Đã xóa báo cáo sự cố.";
        return VeDanhSach();
    }

    private async Task NapThietBiAsync(BaoCaoSuCoFormViewModel vm, NguoiThaoTac nguoi)
    {
        var ds = await _service.LayThietBiChoChonAsync(nguoi, vm.MaThietBi);
        vm.ThietBiOptions = ds.Select(x => new SelectListItem(x.Ten, x.Ma.ToString())).ToList();
    }
}
