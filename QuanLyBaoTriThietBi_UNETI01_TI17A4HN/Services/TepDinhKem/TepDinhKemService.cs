// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Nội dung: STT 53 - Service triển khai Hệ thống Upload tệp đính kèm đa hình (TepDinhKem)
#nullable enable
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class TepDinhKemService : ITepDinhKemService
{
    private const long DungLuongToiDaBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> DuoiFileAnhHopLe = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private static readonly HashSet<string> DuoiFileCam = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".sh", ".ps1", ".dll", ".com", ".vbs", ".js",
        ".jsp", ".asp", ".aspx", ".php", ".py", ".cgi", ".jar", ".msi", ".scr", ".pif", ".reg"
    };

    private readonly ITepDinhKemRepository _repo;
    private readonly IWebHostEnvironment _env;

    public TepDinhKemService(ITepDinhKemRepository repo, IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }

    public async Task<ServiceResult<TepDinhKemItemViewModel>> UploadTepAsync(
        IFormFile file,
        LoaiDoiTuongDinhKemEnum loaiDoiTuong,
        int maDoiTuong,
        int? maTaiKhoanUpload,
        bool chiChapNhanAnh = true
    )
    {
        if (file == null || file.Length == 0)
            return ServiceResult<TepDinhKemItemViewModel>.Fail("Vui lòng chọn tệp tin cần tải lên.");

        // 1. Kiểm tra kích thước tệp
        if (file.Length > DungLuongToiDaBytes)
            return ServiceResult<TepDinhKemItemViewModel>.Fail(
                $"Kích thước tệp \"{file.FileName}\" vượt quá giới hạn 5MB cho phép."
            );

        // 2. Kiểm tra định dạng phần mở rộng
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || DuoiFileCam.Contains(ext))
            return ServiceResult<TepDinhKemItemViewModel>.Fail(
                $"Tệp \"{file.FileName}\" có định dạng không an toàn và bị chặn."
            );

        if (chiChapNhanAnh && !DuoiFileAnhHopLe.Contains(ext))
            return ServiceResult<TepDinhKemItemViewModel>.Fail(
                $"Chỉ chấp nhận các định dạng hình ảnh (.jpg, .jpeg, .png, .webp, .gif)."
            );

        // 3. Kiểm tra đối tượng cha tồn tại (ràng buộc quan hệ đa hình)
        var tonTai = await _repo.KiemTraDoiTuongTonTaiAsync(loaiDoiTuong, maDoiTuong);
        if (!tonTai)
            return ServiceResult<TepDinhKemItemViewModel>.Fail(
                $"Đối tượng cha ({loaiDoiTuong} #{maDoiTuong}) không tồn tại trong hệ thống."
            );

        // 4. Lưu tệp vật lý an toàn vào wwwroot/uploads/{loaiDoiTuong}/
        var subFolder = loaiDoiTuong.ToString().ToLowerInvariant();
        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        var tenFileLuu = $"{Guid.NewGuid():N}{ext}";
        var duongDanVatLy = Path.Combine(uploadFolder, tenFileLuu);

        try
        {
            using (var stream = new FileStream(duongDanVatLy, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
        catch (Exception ex)
        {
            return ServiceResult<TepDinhKemItemViewModel>.Fail(
                $"Lỗi khi lưu tệp tin lên máy chủ: {ex.Message}"
            );
        }

        // 5. Ghi bản ghi vào CSDL
        var duongDanWeb = $"/uploads/{subFolder}/{tenFileLuu}";
        var tep = new TepDinhKem
        {
            LoaiDoiTuong = loaiDoiTuong,
            MaDoiTuong = maDoiTuong,
            DuongDanFile = duongDanWeb,
            TenFileGoc = Path.GetFileName(file.FileName),
            NgayUpload = DateTime.Now,
            MaTaiKhoanUpload = maTaiKhoanUpload
        };

        await _repo.AddAsync(tep);
        await _repo.SaveChangesAsync();

        return ServiceResult<TepDinhKemItemViewModel>.Ok(new TepDinhKemItemViewModel
        {
            MaTepDinhKem = tep.MaTepDinhKem,
            LoaiDoiTuong = tep.LoaiDoiTuong,
            MaDoiTuong = tep.MaDoiTuong,
            DuongDanFile = tep.DuongDanFile,
            TenFileGoc = tep.TenFileGoc ?? string.Empty,
            NgayUpload = tep.NgayUpload,
            MaTaiKhoanUpload = tep.MaTaiKhoanUpload
        });
    }

    public async Task<ServiceResult<List<TepDinhKemItemViewModel>>> UploadNhieuTepAsync(
        IEnumerable<IFormFile> files,
        LoaiDoiTuongDinhKemEnum loaiDoiTuong,
        int maDoiTuong,
        int? maTaiKhoanUpload,
        bool chiChapNhanAnh = true
    )
    {
        var ketQua = new List<TepDinhKemItemViewModel>();
        var danhSachFiles = files?.Where(f => f.Length > 0).ToList();

        if (danhSachFiles == null || danhSachFiles.Count == 0)
            return ServiceResult<List<TepDinhKemItemViewModel>>.Ok(ketQua);

        foreach (var file in danhSachFiles)
        {
            var res = await UploadTepAsync(file, loaiDoiTuong, maDoiTuong, maTaiKhoanUpload, chiChapNhanAnh);
            if (!res.Succeeded)
                return ServiceResult<List<TepDinhKemItemViewModel>>.Fail(res.ErrorMessage);

            if (res.Data != null)
                ketQua.Add(res.Data);
        }

        return ServiceResult<List<TepDinhKemItemViewModel>>.Ok(ketQua);
    }

    public async Task<List<TepDinhKemItemViewModel>> LayDanhSachTheoDoiTuongAsync(
        LoaiDoiTuongDinhKemEnum loaiDoiTuong,
        int maDoiTuong
    )
    {
        var danhSach = await _repo.LayTheoDoiTuongAsync(loaiDoiTuong, maDoiTuong);
        return danhSach.Select(t => new TepDinhKemItemViewModel
        {
            MaTepDinhKem = t.MaTepDinhKem,
            LoaiDoiTuong = t.LoaiDoiTuong,
            MaDoiTuong = t.MaDoiTuong,
            DuongDanFile = t.DuongDanFile,
            TenFileGoc = t.TenFileGoc ?? string.Empty,
            NgayUpload = t.NgayUpload,
            MaTaiKhoanUpload = t.MaTaiKhoanUpload,
            TenNguoiUpload = t.TaiKhoanUpload?.HoTen ?? t.TaiKhoanUpload?.TenDangNhap
        }).ToList();
    }

    public async Task<TepDinhKemItemViewModel?> LayTheoMaAsync(int maTepDinhKem)
    {
        var t = await _repo.GetByIdAsync(maTepDinhKem);
        if (t == null) return null;

        return new TepDinhKemItemViewModel
        {
            MaTepDinhKem = t.MaTepDinhKem,
            LoaiDoiTuong = t.LoaiDoiTuong,
            MaDoiTuong = t.MaDoiTuong,
            DuongDanFile = t.DuongDanFile,
            TenFileGoc = t.TenFileGoc ?? string.Empty,
            NgayUpload = t.NgayUpload,
            MaTaiKhoanUpload = t.MaTaiKhoanUpload
        };
    }

    public async Task<ServiceResult> XoaTepAsync(int maTepDinhKem)
    {
        var tep = await _repo.GetByIdAsync(maTepDinhKem);
        if (tep == null)
            return ServiceResult.Fail("Tệp đính kèm không tồn tại hoặc đã bị xóa.");

        // Xóa file vật lý trên đĩa
        XoaTepVatLy(tep.DuongDanFile);

        // Xóa bản ghi trong DB
        _repo.Remove(tep);
        await _repo.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> XoaToanBoTheoDoiTuongAsync(
        LoaiDoiTuongDinhKemEnum loaiDoiTuong,
        int maDoiTuong
    )
    {
        var danhSach = await _repo.LayTheoDoiTuongAsync(loaiDoiTuong, maDoiTuong);
        foreach (var tep in danhSach)
        {
            XoaTepVatLy(tep.DuongDanFile);
            _repo.Remove(tep);
        }

        if (danhSach.Count > 0)
            await _repo.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private void XoaTepVatLy(string duongDanWeb)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(duongDanWeb)) return;

            var relativePath = duongDanWeb.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Không để lỗi I/O file vật lý làm gián đoạn luồng nghiệp vụ
        }
    }
}
