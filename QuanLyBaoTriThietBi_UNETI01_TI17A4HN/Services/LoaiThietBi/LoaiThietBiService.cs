// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Service Loại thiết bị (nghiệp vụ: không trùng tên, chu kỳ > 0, không xóa khi đang có thiết bị)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class LoaiThietBiService : ILoaiThietBiService
{
    private readonly ILoaiThietBiRepository _repo;

    public LoaiThietBiService(ILoaiThietBiRepository repo) => _repo = repo;

    public async Task<LoaiThietBiIndexViewModel> LayDanhSachAsync(string? tuKhoa, bool? trangThai)
        => new()
        {
            Items = await _repo.TimKiemAsync(tuKhoa, trangThai),
            TuKhoa = tuKhoa,
            TrangThai = trangThai
        };

    public Task<LoaiThietBi?> LayTheoIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<ServiceResult> ThemAsync(LoaiThietBi model)
    {
        ChuanHoa(model);
        var loi = await KiemTraAsync(model, null);
        if (!loi.Succeeded) return loi;

        await _repo.AddAsync(model);
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SuaAsync(LoaiThietBi model)
    {
        var hienTai = await _repo.GetByIdAsync(model.MaLoaiThietBi);
        if (hienTai is null)
            return ServiceResult.Fail("Không tìm thấy loại thiết bị.");

        ChuanHoa(model);
        var loi = await KiemTraAsync(model, model.MaLoaiThietBi);
        if (!loi.Succeeded) return loi;

        // Chỉ chép các trường được phép sửa (tránh over-posting)
        hienTai.TenLoaiThietBi = model.TenLoaiThietBi;
        hienTai.ChuKyBaoTriMacDinhThang = model.ChuKyBaoTriMacDinhThang;
        hienTai.MoTa = model.MoTa;
        hienTai.TrangThai = model.TrangThai;

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<bool>> DoiTrangThaiAsync(int id)
    {
        var loai = await _repo.GetByIdAsync(id);
        if (loai is null)
            return ServiceResult<bool>.Fail("Không tìm thấy loại thiết bị.");

        loai.TrangThai = !loai.TrangThai;
        await _repo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(loai.TrangThai);
    }

    public async Task<ServiceResult> XoaAsync(int id)
    {
        var loai = await _repo.GetByIdAsync(id);
        if (loai is null)
            return ServiceResult.Fail("Không tìm thấy loại thiết bị.");

        var soThietBi = await _repo.DemThietBiAsync(id);
        if (soThietBi > 0)
            return ServiceResult.Fail(
                $"Không thể xóa: loại thiết bị \"{loai.TenLoaiThietBi}\" đang có {soThietBi} thiết bị sử dụng. Hãy chuyển sang Ngừng sử dụng.");

        try
        {
            _repo.Remove(loai);
            await _repo.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail("Không thể xóa: loại thiết bị còn dữ liệu liên quan.");
        }
        return ServiceResult.Ok();
    }

    // ---------- hàm nội bộ ----------
    private static void ChuanHoa(LoaiThietBi model)
    {
        model.TenLoaiThietBi = (model.TenLoaiThietBi ?? string.Empty).Trim();
        model.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();
    }

    private async Task<ServiceResult> KiemTraAsync(LoaiThietBi model, int? boQuaId)
    {
        var kq = ServiceResult.Ok();

        if (await _repo.TenDaTonTaiAsync(model.TenLoaiThietBi, boQuaId))
            kq.AddError("Tên loại thiết bị đã tồn tại", nameof(LoaiThietBi.TenLoaiThietBi));

        if (model.ChuKyBaoTriMacDinhThang <= 0)
            kq.AddError("Chu kỳ bảo trì phải lớn hơn 0 (tháng)", nameof(LoaiThietBi.ChuKyBaoTriMacDinhThang));

        return kq;
    }
}
