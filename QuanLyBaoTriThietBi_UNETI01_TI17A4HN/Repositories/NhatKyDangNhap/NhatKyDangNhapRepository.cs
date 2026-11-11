// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý Hệ thống & Danh mục dùng chung - NhatKyDangNhapRepository.cs
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class NhatKyDangNhapRepository : Repository<NhatKyDangNhap>, INhatKyDangNhapRepository
{
    public NhatKyDangNhapRepository(AppDbContext context) : base(context) { }

    private IQueryable<NhatKyDangNhap> BuildFilteredQuery(NhatKyDangNhapBoLoc boLoc)
    {
        var query = QueryNoTracking();

        if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
        {
            var k = boLoc.TuKhoa.Trim();
            query = query.Where(x =>
                x.TenDangNhapNhap.Contains(k)
                || (x.TaiKhoan != null && x.TaiKhoan.HoTen.Contains(k))
                || (x.DiaChiIP != null && x.DiaChiIP.Contains(k)));
        }

        if (boLoc.KetQua.HasValue)
        {
            query = query.Where(x => x.KetQua == boLoc.KetQua.Value);
        }

        if (boLoc.TuNgay.HasValue)
        {
            var tuNgay = boLoc.TuNgay.Value.Date;
            query = query.Where(x => x.ThoiGian >= tuNgay);
        }

        if (boLoc.DenNgay.HasValue)
        {
            var denNgay = boLoc.DenNgay.Value.Date.AddDays(1);
            query = query.Where(x => x.ThoiGian < denNgay);
        }

        return query;
    }

    private static IQueryable<NhatKyDangNhapRow> ProjectToRow(IQueryable<NhatKyDangNhap> query)
    {
        return query
            .OrderByDescending(x => x.ThoiGian)
            .Select(x => new NhatKyDangNhapRow(
                x.MaNhatKy,
                x.MaTaiKhoan,
                x.TenDangNhapNhap,
                x.TaiKhoan != null ? x.TaiKhoan.HoTen : null,
                x.TaiKhoan != null ? (VaiTroEnum?)x.TaiKhoan.VaiTro : null,
                x.ThoiGian,
                x.KetQua,
                x.DiaChiIP
            ));
    }

    public async Task<PagedResult<NhatKyDangNhapRow>> LayDanhSachPagedAsync(NhatKyDangNhapBoLoc boLoc)
    {
        var query = BuildFilteredQuery(boLoc);
        var projected = ProjectToRow(query);
        return await PagedResult<NhatKyDangNhapRow>.CreateAsync(projected, boLoc.Page, boLoc.PageSize);
    }

    public async Task<List<NhatKyDangNhapRow>> LayTatCaChoXuatExcelAsync(NhatKyDangNhapBoLoc boLoc)
    {
        var query = BuildFilteredQuery(boLoc);
        var projected = ProjectToRow(query);
        return await projected.ToListAsync();
    }

    public async Task<NhatKyDangNhapThongKe> LayThongKeAsync()
    {
        var nhom = await QueryNoTracking()
            .GroupBy(x => x.KetQua)
            .Select(g => new { KetQua = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        var thongKe = new NhatKyDangNhapThongKe();
        foreach (var item in nhom)
        {
            switch (item.KetQua)
            {
                case KetQuaDangNhapEnum.ThanhCong:
                    thongKe.SoThanhCong = item.SoLuong;
                    break;
                case KetQuaDangNhapEnum.SaiMatKhau:
                    thongKe.SoSaiMatKhau = item.SoLuong;
                    break;
                case KetQuaDangNhapEnum.TaiKhoanBiKhoa:
                    thongKe.SoBiKhoa = item.SoLuong;
                    break;
                case KetQuaDangNhapEnum.KhongTonTaiTaiKhoan:
                    thongKe.SoKhongTonTai = item.SoLuong;
                    break;
            }
        }
        thongKe.TongSo = thongKe.SoThanhCong + thongKe.SoSaiMatKhau + thongKe.SoBiKhoa + thongKe.SoKhongTonTai;
        return thongKe;
    }
}
