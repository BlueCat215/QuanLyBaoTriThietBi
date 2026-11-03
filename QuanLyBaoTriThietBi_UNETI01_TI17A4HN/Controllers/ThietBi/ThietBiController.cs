// Controller CRUD Thiết bị + tìm kiếm (chỉ nhận request, gọi Service, trả View)
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

    public async Task<IActionResult> Index(ThietBiBoLoc boLoc)
    {
        var vm = await _service.LayDanhSachAsync(boLoc);

        // Giá trị query string sai kiểu (ví dụ ngày không đọc được) bị bỏ qua -> báo cho người dùng biết
        if (!ModelState.IsValid)
            vm.CanhBao ??= "Một số điều kiện lọc không hợp lệ nên đã được bỏ qua.";

        return View(vm);
    }

    /// <summary>
    /// STT 61: Xuất toàn bộ danh sách thiết bị theo bộ lọc ra file Excel (.xlsx)
    /// </summary>
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

    /// <summary>
    /// STT 54: Tải lên thêm ảnh thực tế cho thiết bị từ trang Chi tiết thiết bị
    /// </summary>
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

    /// <summary>
    /// STT 54: Xóa ảnh thực tế của thiết bị
    /// </summary>
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

    public async Task<IActionResult> Details(int id)
    {
        var vm = await _service.LayChiTietAsync(id);
        if (vm is null)
            return NotFound();

        // STT 56: Sinh mã QR động trỏ tới URL chi tiết thiết bị
        var urlChiTiet = Url.Action("Details", "ThietBi", new { id }, Request.Scheme) 
            ?? $"{Request.Scheme}://{Request.Host}/ThietBi/Details/{id}";
        vm.QrUrl = urlChiTiet;
        vm.QrCodeDataUri = QRCodeHelper.TaoDataUriPng(urlChiTiet, pixelsPerModule: 5);

        return View(vm);
    }

    /// <summary>
    /// STT 56: Tải về ảnh mã QR của thiết bị (.png)
    /// </summary>
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

    /// <summary>
    /// STT 56: Màn hình In tem nhãn tài sản dán máy (Print Template chuẩn 70mm x 40mm)
    /// </summary>
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

    /// <summary>
    /// STT 41: Lịch sử thiết bị - hiển thị lịch sử kế hoạch bảo trì, sự cố, phiếu xử lý, KTV đã phụ trách, ngày hoàn thành, chi phí lũy kế
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LichSu(int id)
    {
        var vm = await _chiTietService.LayLichSuThietBiAsync(id);
        if (vm is null)
            return NotFound();

        return View(vm);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrKyThuatVien)]
    public async Task<IActionResult> Create()
    {
        var vm = new ThietBiFormViewModel();
        await NapDanhMucAsync(vm);
        return View(vm);
    }

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



    /// <param name="returnUrl">Địa chỉ nội bộ để quay về sau khi đổi (danh sách giữ nguyên Tìm kiếm/Lọc/Sắp xếp, hoặc trang chi tiết).</param>
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

        // Chỉ chuyển hướng về địa chỉ nội bộ (chống open redirect)
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

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
