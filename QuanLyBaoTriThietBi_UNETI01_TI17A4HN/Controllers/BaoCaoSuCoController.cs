// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Controller CRUD Báo cáo sự cố (người dùng gửi + xem báo cáo của mình, Admin xem tất cả)
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

// TODO tuần 3 (phân quyền): thêm [Authorize] và đặt DungTaiKhoanXemThuKhiChuaDangNhap = false
// khi SV1 hoàn thành đăng nhập (việc 12). Index = Admin; Create/CuaToi/Details = mọi người đã đăng nhập.
public class BaoCaoSuCoController : Controller
{
    /// TẠM THỜI để chạy thử khi CHƯA có đăng nhập: coi như Admin và gửi báo cáo bằng tài khoản Người sử dụng đầu tiên.
    /// Đặt = false khi đăng nhập đã hoạt động (giống cờ XemTruocMenuAdminKhiChuaDangNhap ở _Sidebar).
    private const bool DungTaiKhoanXemThuKhiChuaDangNhap = true;

    private readonly IBaoCaoSuCoService _service;

    public BaoCaoSuCoController(IBaoCaoSuCoService service) => _service = service;

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
                TempData["ThanhCong"] = "Đã cập nhật báo cáo sự cố.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ModelState.AddServiceErrors(result);
        }

        await NapThietBiAsync(vm, nguoi);
        return View(vm);
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
