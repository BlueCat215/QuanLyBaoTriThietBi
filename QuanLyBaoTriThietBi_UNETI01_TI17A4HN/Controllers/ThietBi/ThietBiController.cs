// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 2: Quản lý thiết bị - ThietBiController
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Controllers;

[Authorize]
public class ThietBiController : Controller
{
    private readonly IThietBiService _service;
    private readonly IChiTietXuLyService _chiTietService;
    private readonly ITepDinhKemService _tepDinhKemService;

    public ThietBiController(
        IThietBiService service,
        IChiTietXuLyService chiTietService,
        ITepDinhKemService tepDinhKemService
    )
    {
        _service = service;
        _chiTietService = chiTietService;
        _tepDinhKemService = tepDinhKemService;
    }

    // GET: /ThietBi
    public async Task<IActionResult> Index(ThietBiBoLoc boLoc)
    {
        var vm = await _service.LayDanhSachAsync(boLoc);

        if (!ModelState.IsValid)
            vm.CanhBao ??= "Một số điều kiện lọc không hợp lệ nên đã được bỏ qua.";

        return View(vm);
    }

    // GET: /ThietBi/ExportExcel
    [HttpGet]
    public async Task<IActionResult> ExportExcel(ThietBiBoLoc boLoc)
    {
        var danhSach = await _service.LayToanBoDeXuatExcelAsync(boLoc);

        var columns = new List<ExcelColumn<ThietBiRow>>
        {
            new("Mã thiết bị", x => UiHelper.Ma("TB", x.MaThietBi), alignment: ClosedXML.Excel.XLAlignmentHorizontalValues.Center),
            new("Tên thiết bị", x => x.TenThietBi),
            new("Số Serial", x => x.SoSerial),
            new("Loại thiết bị", x => x.TenLoaiThietBi),
            new("Khu vực", x => x.TenKhuVuc),
            new("Tình trạng", x => UiHelper.Badge(x.TinhTrang).Text, alignment: ClosedXML.Excel.XLAlignmentHorizontalValues.Center),
            new("Hạn bảo trì tiếp", x => x.NgayBaoTriTiepTheo, numberFormat: "dd/MM/yyyy", alignment: ClosedXML.Excel.XLAlignmentHorizontalValues.Center)
        };

        var nguoiXuat = User.Identity?.Name ?? "Hệ thống";
        var bytes = ExcelExportHelper.Export(
            sheetName: "Danh sách Thiết bị",
            reportTitle: "BÁO CÁO DANH SÁCH THIẾT BỊ BẢO TRÌ",
            data: danhSach,
            columns: columns,
            nguoiXuat: nguoiXuat,
            coCotStt: true
        );

        var fileName = $"DanhSachThietBi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(bytes, ExcelExportHelper.ContentType, fileName);
    }

    // POST: /ThietBi/UploadAnh/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> UploadAnh(int id, List<IFormFile> files)
    {
        if (files == null || files.Count == 0 || files.All(f => f.Length == 0))
        {
            TempData["Loi"] = "Vui lòng chọn ít nhất một tệp ảnh để tải lên.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan);
        var result = await _tepDinhKemService.UploadNhieuTepAsync(
            files,
            LoaiDoiTuongDinhKemEnum.ThietBi,
            id,
            maTaiKhoan,
            chiChapNhanAnh: true
        );

        if (result.Succeeded)
        {
            var soLuong = result.Data?.Count ?? 0;
            TempData["ThanhCong"] = $"Đã tải lên thành công {soLuong} ảnh thực tế cho thiết bị.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /ThietBi/XoaAnh/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> XoaAnh(int id, int maTepDinhKem, string? returnUrl)
    {
        var result = await _tepDinhKemService.XoaTepAsync(maTepDinhKem);
        if (result.Succeeded)
        {
            TempData["ThanhCong"] = "Đã xóa ảnh đính kèm thành công.";
        }
        else
        {
            TempData["Loi"] = result.ErrorMessage;
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /ThietBi/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _service.LayChiTietAsync(id);
        if (vm is null)
            return NotFound();

        var urlChiTiet = Url.Action("Details", "ThietBi", new { id }, Request.Scheme) 
            ?? $"{Request.Scheme}://{Request.Host}/ThietBi/Details/{id}";
        vm.QrUrl = urlChiTiet;
        vm.QrCodeDataUri = QRCodeHelper.TaoDataUriPng(urlChiTiet, pixelsPerModule: 5);

        return View(vm);
    }

    // GET: /ThietBi/QrCode/{id}
    [HttpGet]
    public async Task<IActionResult> QrCode(int id)
    {
        var tb = await _service.LayChiTietAsync(id);
        if (tb is null)
            return NotFound();

        var urlChiTiet = Url.Action("Details", "ThietBi", new { id }, Request.Scheme)
            ?? $"{Request.Scheme}://{Request.Host}/ThietBi/Details/{id}";

        var pngBytes = QRCodeHelper.TaoBytesPng(urlChiTiet, pixelsPerModule: 10);
        var fileName = $"QR_{tb.ThietBi.SoSerial}_{id}.png";
        return File(pngBytes, "image/png", fileName);
    }

    // GET: /ThietBi/InTemNhan/{id}
    [HttpGet]
    public async Task<IActionResult> InTemNhan(int id)
    {
        var chiTietVm = await _service.LayChiTietAsync(id);
        if (chiTietVm is null)
            return NotFound();

        var urlChiTiet = Url.Action("Details", "ThietBi", new { id }, Request.Scheme)
            ?? $"{Request.Scheme}://{Request.Host}/ThietBi/Details/{id}";

        var vm = new ThietBiTemNhanViewModel
        {
            ThietBi = chiTietVm.ThietBi,
            QrCodeDataUri = QRCodeHelper.TaoDataUriPng(urlChiTiet, pixelsPerModule: 6),
            ChiTietUrl = urlChiTiet,
            NgayIn = DateTime.Now
        };

        return View(vm);
    }

    // GET: /ThietBi/LichSu/{id}
    [HttpGet]
    public async Task<IActionResult> LichSu(int id)
    {
        var vm = await _chiTietService.LayLichSuThietBiAsync(id);
        if (vm is null)
            return NotFound();

        return View(vm);
    }

    // GET: /ThietBi/Create
    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> Create()
    {
        var vm = new ThietBiFormViewModel();
        await NapDanhMucAsync(vm);
        return View(vm);
    }

    // POST: /ThietBi/Create
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> Create(ThietBiFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan);
            var result = await _service.ThemAsync(vm, maTaiKhoan);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã thêm thiết bị thành công.";
                return RedirectToAction(nameof(Details), new { id = result.Data });
            }
            ModelState.AddServiceErrors(result);
        }

        await NapDanhMucAsync(vm);
        return View(vm);
    }

    // GET: /ThietBi/Edit/{id}
    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _service.LayDeSuaAsync(id);
        if (vm is null)
            return NotFound();

        await NapDanhMucAsync(vm);
        return View(vm);
    }

    // POST: /ThietBi/Edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> Edit(int id, ThietBiFormViewModel vm)
    {
        if (id != vm.MaThietBi)
            return NotFound();

        if (ModelState.IsValid)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan);
            var result = await _service.SuaAsync(vm, maTaiKhoan);
            if (result.Succeeded)
            {
                TempData["ThanhCong"] = "Đã cập nhật thiết bị thành công.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ModelState.AddServiceErrors(result);
        }

        await NapDanhMucAsync(vm);
        return View(vm);
    }

    // POST: /ThietBi/DoiTinhTrang/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> DoiTinhTrang(
        int id,
        TinhTrangThietBiEnum tinhTrang,
        string? returnUrl
    )
    {
        var result = await _service.DoiTinhTrangAsync(id, tinhTrang);
        if (result.Succeeded)
            TempData["ThanhCong"] =
                $"Đã chuyển tình trạng thiết bị sang \"{UiHelper.Badge(result.Data).Text}\".";
        else
            TempData["Loi"] = result.ErrorMessage;

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    // POST: /ThietBi/Delete/{id}
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.XoaAsync(id);
        if (!result.Succeeded)
        {
            TempData["Loi"] = result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["ThanhCong"] = "Đã xóa thiết bị.";
        return RedirectToAction(nameof(Index));
    }

    private async Task NapDanhMucAsync(ThietBiFormViewModel vm)
    {
        var loai = await _service.LayLoaiChoChonAsync(vm.MaLoaiThietBi);
        var khuVuc = await _service.LayKhuVucChoChonAsync(vm.MaKhuVuc);

        vm.LoaiOptions = loai.Select(x => new SelectListItem(x.Ten, x.Ma.ToString())).ToList();
        vm.KhuVucOptions = khuVuc.Select(x => new SelectListItem(x.Ten, x.Ma.ToString())).ToList();
    }
}
