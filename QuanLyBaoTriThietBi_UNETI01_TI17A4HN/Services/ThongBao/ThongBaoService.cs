// Họ tên: Phạm Việt Hoàn - Nguyễn Khắc Hiếu
// MSSV: 23103100207 - 23103100225
// Nội dung: Module 3 & 5 - STT 59: Service Thông báo nội bộ (In-App Notification)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class ThongBaoService : IThongBaoService
{
    private readonly IThongBaoRepository _repo;
    private readonly AppDbContext _context;

    public ThongBaoService(IThongBaoRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<ThongBaoNhanhViewModel> LayThongBaoNhanhAsync(int maTaiKhoan)
    {
        var soChuaDoc = await _repo.DemSoChuaDocAsync(maTaiKhoan);
        var danhSach = await _repo.LayDanhSachGanNhatAsync(maTaiKhoan, 10);

        return new ThongBaoNhanhViewModel
        {
            SoChuaDoc = soChuaDoc,
            DanhSach = danhSach
        };
    }

    public async Task<ThongBaoIndexViewModel> LayDanhSachAsync(int maTaiKhoan, bool? chiChuaDoc, int trang = 1)
    {
        var paged = await _repo.LayPhanTrangAsync(maTaiKhoan, chiChuaDoc, trang, 15);
        var soChuaDoc = await _repo.DemSoChuaDocAsync(maTaiKhoan);
        var tongSo = await _context.ThongBaos
            .CountAsync(t => t.MaTaiKhoanNhan == maTaiKhoan && t.Kenh == KenhThongBaoEnum.HeThong);

        return new ThongBaoIndexViewModel
        {
            PagedItems = paged,
            ChiChuaDoc = chiChuaDoc,
            SoChuaDoc = soChuaDoc,
            TongSo = tongSo
        };
    }

    public Task<bool> DanhDauDaDocAsync(int maThongBao, int maTaiKhoan) =>
        _repo.DanhDauDaDocAsync(maThongBao, maTaiKhoan);

    public Task<int> DanhDauTatCaDaDocAsync(int maTaiKhoan) =>
        _repo.DanhDauTatCaDaDocAsync(maTaiKhoan);

    public async Task TaoThongBaoHeThongAsync(int maTaiKhoanNhan, string noiDung, int? maThietBi = null)
    {
        if (maTaiKhoanNhan <= 0 || string.IsNullOrWhiteSpace(noiDung)) return;

        var tb = new ThongBao
        {
            Kenh = KenhThongBaoEnum.HeThong,
            MaTaiKhoanNhan = maTaiKhoanNhan,
            MaThietBi = maThietBi,
            NoiDung = noiDung.Trim(),
            NgayTao = DateTime.Now,
            NgayGui = DateTime.Now,
            TrangThai = TrangThaiGuiThongBaoEnum.DaGui,
            DaDoc = false
        };

        await _repo.ThemThongBaoAsync(tb);
    }

    public async Task TaoThongBaoChoAdminAsync(string noiDung, int? maThietBi = null)
    {
        if (string.IsNullOrWhiteSpace(noiDung)) return;

        var adminIds = await _context.TaiKhoans
            .AsNoTracking()
            .Where(t => t.VaiTro == VaiTroEnum.Admin && t.TrangThai)
            .Select(t => t.MaTaiKhoan)
            .ToListAsync();

        if (adminIds.Count == 0) return;

        var now = DateTime.Now;
        var danhSach = adminIds.Select(id => new ThongBao
        {
            Kenh = KenhThongBaoEnum.HeThong,
            MaTaiKhoanNhan = id,
            MaThietBi = maThietBi,
            NoiDung = noiDung.Trim(),
            NgayTao = now,
            NgayGui = now,
            TrangThai = TrangThaiGuiThongBaoEnum.DaGui,
            DaDoc = false
        });

        await _repo.ThemNhieuThongBaoAsync(danhSach);
    }

    public async Task TaoThongBaoChoAdminVaKtvAsync(string noiDung, int? maThietBi = null, int? maKhuVuc = null)
    {
        if (string.IsNullOrWhiteSpace(noiDung)) return;

        var userIds = await _context.TaiKhoans
            .AsNoTracking()
            .Where(t => t.TrangThai && (t.VaiTro == VaiTroEnum.Admin || t.VaiTro == VaiTroEnum.KyThuatVien))
            .Select(t => t.MaTaiKhoan)
            .Distinct()
            .ToListAsync();

        if (userIds.Count == 0) return;

        var now = DateTime.Now;
        var danhSach = userIds.Select(id => new ThongBao
        {
            Kenh = KenhThongBaoEnum.HeThong,
            MaTaiKhoanNhan = id,
            MaThietBi = maThietBi,
            NoiDung = noiDung.Trim(),
            NgayTao = now,
            NgayGui = now,
            TrangThai = TrangThaiGuiThongBaoEnum.DaGui,
            DaDoc = false
        });

        await _repo.ThemNhieuThongBaoAsync(danhSach);
    }

    public async Task TaoThongBaoChoKtvAsync(int maKyThuatVien, string noiDung, int? maThietBi = null)
    {
        if (maKyThuatVien <= 0 || string.IsNullOrWhiteSpace(noiDung)) return;

        var ktv = await _context.KyThuatViens
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.MaKyThuatVien == maKyThuatVien);

        if (ktv?.MaTaiKhoan == null) return;

        await TaoThongBaoHeThongAsync(ktv.MaTaiKhoan.Value, noiDung, maThietBi);
    }
}
