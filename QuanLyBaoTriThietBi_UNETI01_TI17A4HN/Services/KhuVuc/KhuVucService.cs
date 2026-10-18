// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Service Khu vực (nghiệp vụ: không trùng tên, không xóa khi còn thiết bị/tài khoản)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class KhuVucService : IKhuVucService
{
    private readonly IKhuVucRepository _repo;

    public KhuVucService(IKhuVucRepository repo) => _repo = repo;

    public async Task<KhuVucIndexViewModel> LayDanhSachAsync(string? tuKhoa, bool? trangThai)
        => new()
        {
            Items = await _repo.TimKiemAsync(tuKhoa, trangThai),
            TuKhoa = tuKhoa,
            TrangThai = trangThai
        };

    public Task<KhuVuc?> LayTheoIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<ServiceResult> ThemAsync(KhuVuc model)
    {
        ChuanHoa(model);
        var loi = await KiemTraAsync(model, null);
        if (!loi.Succeeded) return loi;

        await _repo.AddAsync(model);
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SuaAsync(KhuVuc model)
    {
        var hienTai = await _repo.GetByIdAsync(model.MaKhuVuc);
        if (hienTai is null)
            return ServiceResult.Fail("Không tìm thấy khu vực.");

        ChuanHoa(model);
        var loi = await KiemTraAsync(model, model.MaKhuVuc);
        if (!loi.Succeeded) return loi;

        hienTai.TenKhuVuc = model.TenKhuVuc;
        hienTai.ViTri = model.ViTri;
        hienTai.NguoiPhuTrach = model.NguoiPhuTrach;
        hienTai.MoTa = model.MoTa;
        hienTai.TrangThai = model.TrangThai;

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<bool>> DoiTrangThaiAsync(int id)
    {
        var khuVuc = await _repo.GetByIdAsync(id);
        if (khuVuc is null)
            return ServiceResult<bool>.Fail("Không tìm thấy khu vực.");

        khuVuc.TrangThai = !khuVuc.TrangThai;
        await _repo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(khuVuc.TrangThai);
    }

    public async Task<ServiceResult> XoaAsync(int id)
    {
        var khuVuc = await _repo.GetByIdAsync(id);
        if (khuVuc is null)
            return ServiceResult.Fail("Không tìm thấy khu vực.");

        var soThietBi = await _repo.DemThietBiAsync(id);
        var soTaiKhoan = await _repo.DemTaiKhoanAsync(id);
        if (soThietBi > 0 || soTaiKhoan > 0)
            return ServiceResult.Fail(
                $"Không thể xóa: khu vực \"{khuVuc.TenKhuVuc}\" đang có {soThietBi} thiết bị và {soTaiKhoan} tài khoản. Hãy chuyển sang Ngừng hoạt động.");

        try
        {
            _repo.Remove(khuVuc);
            await _repo.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail("Không thể xóa: khu vực còn dữ liệu liên quan.");
        }
        return ServiceResult.Ok();
    }

    // ---------- hàm nội bộ ----------
    private static string? ChuanHoaChuoi(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static void ChuanHoa(KhuVuc model)
    {
        model.TenKhuVuc = (model.TenKhuVuc ?? string.Empty).Trim();
        model.ViTri = ChuanHoaChuoi(model.ViTri);
        model.NguoiPhuTrach = ChuanHoaChuoi(model.NguoiPhuTrach);
        model.MoTa = ChuanHoaChuoi(model.MoTa);
    }

    private async Task<ServiceResult> KiemTraAsync(KhuVuc model, int? boQuaId)
    {
        var kq = ServiceResult.Ok();

        if (await _repo.TenDaTonTaiAsync(model.TenKhuVuc, boQuaId))
            kq.AddError("Tên khu vực đã tồn tại", nameof(KhuVuc.TenKhuVuc));

        return kq;
    }
}
