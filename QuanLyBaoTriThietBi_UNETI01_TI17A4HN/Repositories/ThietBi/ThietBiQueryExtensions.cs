// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 2: Quản lý thiết bị - ThietBiQueryExtensions (LINQ Search, Filter, Sort, Hạn bảo trì)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public static class ThietBiQueryExtensions
{
    private const int SoTuKhoaToiDa = 5;

    public static IQueryable<ThietBi> ApDungLoc(
        this IQueryable<ThietBi> query,
        ThietBiBoLoc boLoc,
        DateTime homNay
    )
    {
        if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
        {
            var cacTu = boLoc.TuKhoa.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            ).Take(SoTuKhoaToiDa);

            foreach (var tu in cacTu)
            {
                query = query.Where(t =>
                    t.TenThietBi.Contains(tu)
                    || t.SoSerial.Contains(tu)
                    || t.KhuVuc!.TenKhuVuc.Contains(tu)
                );
            }
        }

        if (boLoc.MaLoaiThietBi is { } maLoai)
            query = query.Where(t => t.MaLoaiThietBi == maLoai);

        if (boLoc.MaKhuVuc is { } maKhuVuc)
            query = query.Where(t => t.MaKhuVuc == maKhuVuc);

        if (boLoc.TinhTrang is { } tinhTrang)
            query = query.Where(t => t.TinhTrang == tinhTrang);

        if (!boLoc.KhoangNgayKhongHopLe)
        {
            if (boLoc.NgaySuDungTu is { } tu)
            {
                var tuNgay = tu.Date;
                query = query.Where(t => t.NgayDuaVaoSuDung >= tuNgay);
            }
            if (boLoc.NgaySuDungDen is { } den)
            {
                var denNgayKeTiep = den.Date.AddDays(1);
                query = query.Where(t => t.NgayDuaVaoSuDung < denNgayKeTiep);
            }
        }

        if (boLoc.HanBaoTri is { } han)
            query = query.LocTheoHanBaoTri(han, homNay);

        return query;
    }

    public static IQueryable<ThietBi> LocTheoHanBaoTri(
        this IQueryable<ThietBi> query,
        TrangThaiHanBaoTriEnum han,
        DateTime homNay
    )
    {
        var ngayHomNay = homNay.Date;
        var mocChuaDenHan = ngayHomNay.AddDays(UiHelper.NgayCanhBaoSapDenHan + 1);

        query = query.Where(t => t.TinhTrang != TinhTrangThietBiEnum.NgungSuDung);

        return han switch
        {
            TrangThaiHanBaoTriEnum.QuaHan => query.Where(t =>
                (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                < ngayHomNay
            ),

            TrangThaiHanBaoTriEnum.SapDenHan => query.Where(t =>
                (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                    >= ngayHomNay
                && (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                    < mocChuaDenHan
            ),

            _ => query.Where(t =>
                (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                >= mocChuaDenHan
            ),
        };
    }

    public static IQueryable<ThietBi> SapDenHanBaoTri(
        this IQueryable<ThietBi> query,
        DateTime? homNay = null
    ) => query.LocTheoHanBaoTri(TrangThaiHanBaoTriEnum.SapDenHan, homNay ?? DateTime.Today);

    public static IQueryable<ThietBi> QuaHanBaoTri(
        this IQueryable<ThietBi> query,
        DateTime? homNay = null
    ) => query.LocTheoHanBaoTri(TrangThaiHanBaoTriEnum.QuaHan, homNay ?? DateTime.Today);

    public static IQueryable<ThietBi> ChuaDenHanBaoTri(
        this IQueryable<ThietBi> query,
        DateTime? homNay = null
    ) => query.LocTheoHanBaoTri(TrangThaiHanBaoTriEnum.ChuaDenHan, homNay ?? DateTime.Today);

    public static IQueryable<ThietBi> ApDungSapXep(
        this IQueryable<ThietBi> query,
        ThietBiSapXep sapXep
    ) =>
        sapXep switch
        {
            ThietBiSapXep.TenZA => query
                .OrderByDescending(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.NgaySuDungMoiNhat => query
                .OrderByDescending(t => t.NgayDuaVaoSuDung)
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.NgaySuDungCuNhat => query
                .OrderBy(t => t.NgayDuaVaoSuDung)
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.BaoTriSomNhat => query
                .OrderBy(t =>
                    (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                )
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.BaoTriMuonNhat => query
                .OrderByDescending(t =>
                    (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                )
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.ChuKyNganNhat => query
                .OrderBy(t => t.ChuKyBaoTriThang)
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.ChuKyDaiNhat => query
                .OrderByDescending(t => t.ChuKyBaoTriThang)
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            _ => query.OrderBy(t => t.TenThietBi).ThenBy(t => t.MaThietBi),
        };
}
