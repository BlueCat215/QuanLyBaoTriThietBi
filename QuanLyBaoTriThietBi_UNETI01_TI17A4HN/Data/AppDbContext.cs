#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<KhuVuc> KhuVucs => Set<KhuVuc>();
    public DbSet<LoaiThietBi> LoaiThietBis => Set<LoaiThietBi>();
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<NhatKyDangNhap> NhatKyDangNhaps => Set<NhatKyDangNhap>();
    public DbSet<ThietBi> ThietBis => Set<ThietBi>();
    public DbSet<KeHoachBaoTri> KeHoachBaoTris => Set<KeHoachBaoTri>();
    public DbSet<BaoCaoSuCo> BaoCaoSuCos => Set<BaoCaoSuCo>();
    public DbSet<KyThuatVien> KyThuatViens => Set<KyThuatVien>();
    public DbSet<PhieuXuLy> PhieuXuLys => Set<PhieuXuLy>();
    public DbSet<PhanCongXuLy> PhanCongXuLys => Set<PhanCongXuLy>();
    public DbSet<VatTu> VatTus => Set<VatTu>();
    public DbSet<ChiTietXuLy> ChiTietXuLys => Set<ChiTietXuLy>();
    public DbSet<TepDinhKem> TepDinhKems => Set<TepDinhKem>();
    public DbSet<ThongBao> ThongBaos => Set<ThongBao>();

    private static string InList<TEnum>(string column) where TEnum : struct, Enum =>
        $"[{column}] IN ({string.Join(", ", Enum.GetNames<TEnum>().Select(n => $"'{n}'"))})";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<KhuVuc>(e =>
        {
            e.HasIndex(x => x.TenKhuVuc).IsUnique().HasDatabaseName("UQ_KhuVuc_Ten");
        });

        modelBuilder.Entity<LoaiThietBi>(e =>
        {
            e.HasIndex(x => x.TenLoaiThietBi).IsUnique().HasDatabaseName("UQ_LoaiThietBi_Ten");
            e.ToTable(t => t.HasCheckConstraint("CK_LoaiThietBi_ChuKy", "[ChuKyBaoTriMacDinhThang] > 0"));
        });

        modelBuilder.Entity<TaiKhoan>(e =>
        {
            e.HasIndex(x => x.TenDangNhap).IsUnique().HasDatabaseName("UQ_TaiKhoan_TenDangNhap");
            e.Property(x => x.SoLanDangNhapSai).HasDefaultValue(0);
            e.Property(x => x.NgayTaoTaiKhoan).HasDefaultValueSql("getdate()");
            e.HasOne(x => x.KhuVuc).WithMany(k => k.TaiKhoans).HasForeignKey(x => x.MaKhuVuc);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_TaiKhoan_VaiTro", InList<VaiTroEnum>("VaiTro"));
                t.HasCheckConstraint("CK_TaiKhoan_SoLanSai", "[SoLanDangNhapSai] >= 0");
            });
        });

        modelBuilder.Entity<NhatKyDangNhap>(e =>
        {
            e.Property(x => x.ThoiGian).HasDefaultValueSql("getdate()");
            e.HasOne(x => x.TaiKhoan).WithMany(t => t.NhatKyDangNhaps).HasForeignKey(x => x.MaTaiKhoan);
            e.HasIndex(x => x.MaTaiKhoan).HasDatabaseName("IX_NhatKy_MaTaiKhoan");
            e.HasIndex(x => x.ThoiGian).HasDatabaseName("IX_NhatKy_ThoiGian");
            e.ToTable(t => t.HasCheckConstraint("CK_NhatKy_KetQua", InList<KetQuaDangNhapEnum>("KetQua")));
        });

        modelBuilder.Entity<ThietBi>(e =>
        {
            e.HasIndex(x => x.SoSerial).IsUnique().HasDatabaseName("UQ_ThietBi_SoSerial");
            e.HasOne(x => x.LoaiThietBi).WithMany(l => l.ThietBis).HasForeignKey(x => x.MaLoaiThietBi);
            e.HasOne(x => x.KhuVuc).WithMany(k => k.ThietBis).HasForeignKey(x => x.MaKhuVuc);
            e.HasIndex(x => x.MaLoaiThietBi).HasDatabaseName("IX_ThietBi_Loai");
            e.HasIndex(x => x.MaKhuVuc).HasDatabaseName("IX_ThietBi_KhuVuc");
            e.HasIndex(x => x.TinhTrang).HasDatabaseName("IX_ThietBi_TinhTrang");
            e.HasIndex(x => new { x.MaLoaiThietBi, x.MaKhuVuc, x.TinhTrang }).HasDatabaseName("IX_ThietBi_Loc");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ThietBi_ChuKy", "[ChuKyBaoTriThang] > 0");
                t.HasCheckConstraint("CK_ThietBi_TinhTrang", InList<TinhTrangThietBiEnum>("TinhTrang"));
            });
        });

        modelBuilder.Entity<KeHoachBaoTri>(e =>
        {
            e.Property(x => x.TrangThai).HasDefaultValue(TrangThaiKeHoachEnum.ChoThucHien);
            e.HasOne(x => x.ThietBi).WithMany(t => t.KeHoachBaoTris).HasForeignKey(x => x.MaThietBi);
            e.HasIndex(x => x.MaThietBi).HasDatabaseName("IX_KeHoach_ThietBi");
            e.HasIndex(x => x.TrangThai).HasDatabaseName("IX_KeHoach_TrangThai");
            e.HasIndex(x => x.NgayDuKien).HasDatabaseName("IX_KeHoach_NgayDuKien");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_KeHoach_MucDoUuTien", InList<MucDoUuTienEnum>("MucDoUuTien"));
                t.HasCheckConstraint("CK_KeHoach_TrangThai", InList<TrangThaiKeHoachEnum>("TrangThai"));
            });
        });

        modelBuilder.Entity<BaoCaoSuCo>(e =>
        {
            e.Property(x => x.TrangThai).HasDefaultValue(TrangThaiBaoCaoEnum.MoiBao);
            e.HasOne(x => x.ThietBi).WithMany(t => t.BaoCaoSuCos).HasForeignKey(x => x.MaThietBi);
            e.HasOne(x => x.TaiKhoanBaoCao).WithMany(t => t.BaoCaoSuCos).HasForeignKey(x => x.MaTaiKhoanBaoCao);
            e.HasIndex(x => x.MaThietBi).HasDatabaseName("IX_BaoCao_ThietBi");
            e.HasIndex(x => x.MaTaiKhoanBaoCao).HasDatabaseName("IX_BaoCao_TaiKhoan");
            e.HasIndex(x => x.TrangThai).HasDatabaseName("IX_BaoCao_TrangThai");
            e.HasIndex(x => x.MucDoSuCo).HasDatabaseName("IX_BaoCao_MucDo");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_BaoCao_MucDo", InList<MucDoSuCoEnum>("MucDoSuCo"));
                t.HasCheckConstraint("CK_BaoCao_TrangThai", InList<TrangThaiBaoCaoEnum>("TrangThai"));
            });
        });

        modelBuilder.Entity<KyThuatVien>(e =>
        {
            e.HasOne(x => x.TaiKhoan).WithOne(t => t.KyThuatVien).HasForeignKey<KyThuatVien>(x => x.MaTaiKhoan);
            e.HasIndex(x => x.MaTaiKhoan).IsUnique()
                .HasFilter("[MaTaiKhoan] IS NOT NULL")
                .HasDatabaseName("UX_KyThuatVien_MaTaiKhoan");
        });

        modelBuilder.Entity<PhieuXuLy>(e =>
        {
            e.Property(x => x.TrangThai).HasDefaultValue(TrangThaiPhieuXuLyEnum.ChoPhanCong);
            e.HasOne(x => x.BaoCaoSuCo).WithMany(b => b.PhieuXuLys).HasForeignKey(x => x.MaBaoCao);
            e.HasOne(x => x.KeHoachBaoTri).WithMany(k => k.PhieuXuLys).HasForeignKey(x => x.MaKeHoach);
            e.HasIndex(x => x.MaBaoCao).HasDatabaseName("IX_Phieu_BaoCao");
            e.HasIndex(x => x.MaKeHoach).HasDatabaseName("IX_Phieu_KeHoach");
            e.HasIndex(x => x.TrangThai).HasDatabaseName("IX_Phieu_TrangThai");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Phieu_LoaiXuLy", InList<LoaiXuLyEnum>("LoaiXuLy"));
                t.HasCheckConstraint("CK_Phieu_TrangThai", InList<TrangThaiPhieuXuLyEnum>("TrangThai"));
                // Dung 1 trong 2 FK theo LoaiXuLy
                t.HasCheckConstraint("CK_Phieu_Nguon",
                    "([LoaiXuLy] = 'XuLySuCo' AND [MaBaoCao] IS NOT NULL AND [MaKeHoach] IS NULL) " +
                    "OR ([LoaiXuLy] = 'BaoTriDinhKy' AND [MaKeHoach] IS NOT NULL AND [MaBaoCao] IS NULL)");
            });
        });

        modelBuilder.Entity<PhanCongXuLy>(e =>
        {
            e.Property(x => x.TrangThai).HasDefaultValue(TrangThaiPhanCongEnum.DangHieuLuc);
            e.HasOne(x => x.PhieuXuLy).WithMany(p => p.PhanCongXuLys).HasForeignKey(x => x.MaPhieuXuLy);
            e.HasOne(x => x.KyThuatVien).WithMany(k => k.PhanCongXuLys).HasForeignKey(x => x.MaKyThuatVien);
            e.HasIndex(x => x.MaPhieuXuLy).HasDatabaseName("IX_PhanCong_Phieu");
            e.HasIndex(x => x.MaKyThuatVien).HasDatabaseName("IX_PhanCong_KTV");
            e.HasIndex(x => x.TrangThai).HasDatabaseName("IX_PhanCong_TrangThai");
            e.HasIndex(x => new { x.MaPhieuXuLy, x.TrangThai }).HasDatabaseName("IX_PhanCong_Phieu_TT");
            // Moi phieu chi 1 phan cong dang hieu luc
            e.HasIndex(x => x.MaPhieuXuLy).IsUnique()
                .HasFilter("[TrangThai] = 'DangHieuLuc'")
                .HasDatabaseName("UX_PhanCong_DangHieuLuc");
            e.ToTable(t => t.HasCheckConstraint("CK_PhanCong_TrangThai", InList<TrangThaiPhanCongEnum>("TrangThai")));
        });

        // ------------------------------------------------------------ MODULE 5
        modelBuilder.Entity<VatTu>(e =>
        {
            e.HasIndex(x => x.TenVatTu).IsUnique().HasDatabaseName("UQ_VatTu_Ten");
            e.Property(x => x.SoLuongTon).HasDefaultValue(0);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_VatTu_DonGia", "[DonGiaMacDinh] >= 0");
                t.HasCheckConstraint("CK_VatTu_Ton", "[SoLuongTon] >= 0");
            });
        });

        modelBuilder.Entity<ChiTietXuLy>(e =>
        {
            e.HasOne(x => x.PhieuXuLy).WithMany(p => p.ChiTietXuLys).HasForeignKey(x => x.MaPhieuXuLy);
            e.HasOne(x => x.VatTu).WithMany(v => v.ChiTietXuLys).HasForeignKey(x => x.MaVatTu);
            e.HasIndex(x => x.MaPhieuXuLy).HasDatabaseName("IX_ChiTiet_Phieu");
            e.HasIndex(x => x.MaVatTu).HasDatabaseName("IX_ChiTiet_VatTu");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ChiTiet_SoLuong", "[SoLuong] IS NULL OR [SoLuong] > 0");
                t.HasCheckConstraint("CK_ChiTiet_DonGia", "[DonGia] IS NULL OR [DonGia] >= 0");
                t.HasCheckConstraint("CK_ChiTiet_ThanhTien",
                    "[ThanhTien] IS NULL OR ([SoLuong] IS NOT NULL AND [DonGia] IS NOT NULL AND [ThanhTien] = [SoLuong] * [DonGia])");
            });
        });

        modelBuilder.Entity<TepDinhKem>(e =>
        {
            e.Property(x => x.NgayUpload).HasDefaultValueSql("getdate()");
            e.HasOne(x => x.TaiKhoanUpload).WithMany(t => t.TepDinhKems).HasForeignKey(x => x.MaTaiKhoanUpload);
            e.HasIndex(x => new { x.LoaiDoiTuong, x.MaDoiTuong }).HasDatabaseName("IX_TepDinhKem_DoiTuong");
            e.ToTable(t => t.HasCheckConstraint("CK_TepDinhKem_Loai", InList<LoaiDoiTuongDinhKemEnum>("LoaiDoiTuong")));
        });

        modelBuilder.Entity<ThongBao>(e =>
        {
            e.Property(x => x.NgayTao).HasDefaultValueSql("getdate()");
            e.Property(x => x.TrangThai).HasDefaultValue(TrangThaiGuiThongBaoEnum.ChoGui);
            e.Property(x => x.DaDoc).HasDefaultValue(false);
            e.HasOne(x => x.TaiKhoanNhan).WithMany(t => t.ThongBaos).HasForeignKey(x => x.MaTaiKhoanNhan);
            e.HasOne(x => x.ThietBi).WithMany(t => t.ThongBaos).HasForeignKey(x => x.MaThietBi);
            e.HasIndex(x => x.MaTaiKhoanNhan).HasDatabaseName("IX_ThongBao_TaiKhoan");
            e.HasIndex(x => x.TrangThai).HasDatabaseName("IX_ThongBao_TrangThai");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ThongBao_Kenh", InList<KenhThongBaoEnum>("Kenh"));
                t.HasCheckConstraint("CK_ThongBao_TrangThai", InList<TrangThaiGuiThongBaoEnum>("TrangThai"));
            });
        });

        // ------------------------------------------------ QUY UOC CHUNG
        // Luu y: KHONG dung HasDefaultValue(true) cho cot bool (EF coi false la 'chua gan' va ghi de bang true).
        // Gia tri mac dinh cua bool TrangThai duoc gan ngay trong class entity (= true).

        // 1) Moi enum luu dang chuoi nvarchar(255)
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).ToList())
        {
            if (property.ClrType.IsEnum)
            {
                var converterType = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
                property.SetValueConverter((ValueConverter)Activator.CreateInstance(converterType)!);
                property.SetMaxLength(255);
            }
        }

        // 2) Moi FK la Restrict - khong ON DELETE CASCADE (bao toan lich su)
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()).ToList())
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}