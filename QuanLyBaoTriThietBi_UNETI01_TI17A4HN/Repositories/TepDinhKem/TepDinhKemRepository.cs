// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Nội dung: STT 53 - Repository triển khai cho Tệp đính kèm đa hình (TepDinhKem)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class TepDinhKemRepository : Repository<TepDinhKem>, ITepDinhKemRepository
{
    public TepDinhKemRepository(AppDbContext context) : base(context) { }

    public Task<List<TepDinhKem>> LayTheoDoiTuongAsync(LoaiDoiTuongDinhKemEnum loaiDoiTuong, int maDoiTuong)
    {
        return _context.TepDinhKems
            .AsNoTracking()
            .Include(t => t.TaiKhoanUpload)
            .Where(t => t.LoaiDoiTuong == loaiDoiTuong && t.MaDoiTuong == maDoiTuong)
            .OrderByDescending(t => t.NgayUpload)
            .ToListAsync();
    }

    public async Task<bool> KiemTraDoiTuongTonTaiAsync(LoaiDoiTuongDinhKemEnum loaiDoiTuong, int maDoiTuong)
    {
        return loaiDoiTuong switch
        {
            LoaiDoiTuongDinhKemEnum.ThietBi =>
                await _context.ThietBis.AnyAsync(t => t.MaThietBi == maDoiTuong),

            LoaiDoiTuongDinhKemEnum.BaoCaoSuCo =>
                await _context.BaoCaoSuCos.AnyAsync(b => b.MaBaoCao == maDoiTuong),

            LoaiDoiTuongDinhKemEnum.ChiTietXuLy =>
                await _context.ChiTietXuLys.AnyAsync(c => c.MaChiTiet == maDoiTuong),

            _ => false
        };
    }

    public Task<int> DemSoTepAsync(LoaiDoiTuongDinhKemEnum loaiDoiTuong, int maDoiTuong)
    {
        return _context.TepDinhKems
            .CountAsync(t => t.LoaiDoiTuong == loaiDoiTuong && t.MaDoiTuong == maDoiTuong);
    }
}
