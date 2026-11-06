/* =====================================================================
   THIET KE CSDL - He thong Quan ly Bao tri Thiet bi va Xu ly Su co
   De tai 16 - Nhom UNETI16 - SQL Server
   PHIEN BAN 1.0 - CHOT NGAY 03/10/2026  (dong bo voi schema.dbml)
   Chay tren database trong. 14 bang: 10 bat buoc + 4 bang phu (nang cao/bao mat).
   Quy uoc: cot enum luu dang CHUOI (nvarchar + CHECK); trong EF Core dung
            .HasConversion<string>() cho cac cot enum.
   ===================================================================== */

/* ---------------------------------------------------------------------
   MODULE 1: KhuVuc, LoaiThietBi, TaiKhoan, NhatKyDangNhap
   --------------------------------------------------------------------- */
CREATE TABLE [KhuVuc] (
  [MaKhuVuc]      int PRIMARY KEY IDENTITY(1, 1),
  [TenKhuVuc]     nvarchar(100) NOT NULL,
  [ViTri]         nvarchar(255) NULL,
  [NguoiPhuTrach] nvarchar(100) NULL,
  [MoTa]          nvarchar(255) NULL,
  [TrangThai]     bit NOT NULL CONSTRAINT [DF_KhuVuc_TrangThai] DEFAULT (1),
  CONSTRAINT [UQ_KhuVuc_Ten] UNIQUE ([TenKhuVuc])
)
GO

CREATE TABLE [LoaiThietBi] (
  [MaLoaiThietBi]           int PRIMARY KEY IDENTITY(1, 1),
  [TenLoaiThietBi]          nvarchar(100) NOT NULL,
  [ChuKyBaoTriMacDinhThang] int NOT NULL,
  [MoTa]                    nvarchar(255) NULL,
  [TrangThai]               bit NOT NULL CONSTRAINT [DF_LoaiThietBi_TrangThai] DEFAULT (1),
  CONSTRAINT [UQ_LoaiThietBi_Ten] UNIQUE ([TenLoaiThietBi]),
  CONSTRAINT [CK_LoaiThietBi_ChuKy] CHECK ([ChuKyBaoTriMacDinhThang] > 0)
)
GO

CREATE TABLE [TaiKhoan] (
  [MaTaiKhoan]       int PRIMARY KEY IDENTITY(1, 1),
  [TenDangNhap]      nvarchar(50)  NOT NULL,
  [MatKhau]          nvarchar(255) NOT NULL,
  [HoTen]            nvarchar(100) NOT NULL,
  [Email]            nvarchar(100) NOT NULL,
  [VaiTro]           nvarchar(255) NOT NULL,
  [MaKhuVuc]         int NULL,
  [TrangThai]        bit NOT NULL CONSTRAINT [DF_TaiKhoan_TrangThai] DEFAULT (1),
  [MatKhauSalt]      nvarchar(255) NULL,
  [SoLanDangNhapSai] int NOT NULL CONSTRAINT [DF_TaiKhoan_SoLanSai] DEFAULT (0),
  [KhoaDenNgay]      datetime NULL,
  [NgayTaoTaiKhoan]  datetime NOT NULL CONSTRAINT [DF_TaiKhoan_NgayTao] DEFAULT (getdate()),
  [LanDangNhapCuoi]  datetime NULL,
  CONSTRAINT [UQ_TaiKhoan_TenDangNhap] UNIQUE ([TenDangNhap]),
  CONSTRAINT [CK_TaiKhoan_VaiTro] CHECK ([VaiTro] IN ('Admin', 'KyThuatVien', 'NguoiSuDung')),
  CONSTRAINT [CK_TaiKhoan_SoLanSai] CHECK ([SoLanDangNhapSai] >= 0)
)
GO

CREATE TABLE [NhatKyDangNhap] (
  [MaNhatKy]        int PRIMARY KEY IDENTITY(1, 1),
  [MaTaiKhoan]      int NULL,
  [TenDangNhapNhap] nvarchar(50) NOT NULL,
  [ThoiGian]        datetime NOT NULL CONSTRAINT [DF_NhatKy_ThoiGian] DEFAULT (getdate()),
  [KetQua]          nvarchar(255) NOT NULL,
  [DiaChiIP]        nvarchar(45) NULL,
  CONSTRAINT [CK_NhatKy_KetQua] CHECK ([KetQua] IN ('ThanhCong', 'SaiMatKhau', 'TaiKhoanBiKhoa', 'KhongTonTaiTaiKhoan'))
)
GO

/* ---------------------------------------------------------------------
   MODULE 2: ThietBi
   --------------------------------------------------------------------- */
CREATE TABLE [ThietBi] (
  [MaThietBi]         int PRIMARY KEY IDENTITY(1, 1),
  [TenThietBi]        nvarchar(150) NOT NULL,
  [MaLoaiThietBi]     int NOT NULL,
  [MaKhuVuc]          int NOT NULL,
  [SoSerial]          nvarchar(100) NOT NULL,
  [NgayDuaVaoSuDung]  date NOT NULL,
  [ChuKyBaoTriThang]  int NOT NULL,
  [NgayBaoTriGanNhat] date NULL,
  [TinhTrang]         nvarchar(255) NOT NULL,
  [MoTa]              nvarchar(255) NULL,
  CONSTRAINT [UQ_ThietBi_SoSerial] UNIQUE ([SoSerial]),
  CONSTRAINT [CK_ThietBi_ChuKy] CHECK ([ChuKyBaoTriThang] > 0),
  CONSTRAINT [CK_ThietBi_TinhTrang] CHECK ([TinhTrang] IN ('DangHoatDong', 'DangBaoTri', 'TamNgungDoSuCo', 'NgungSuDung'))
)
GO

/* ---------------------------------------------------------------------
   MODULE 3: KeHoachBaoTri
   --------------------------------------------------------------------- */
CREATE TABLE [KeHoachBaoTri] (
  [MaKeHoach]     int PRIMARY KEY IDENTITY(1, 1),
  [MaThietBi]     int NOT NULL,
  [NgayDuKien]    date NOT NULL,
  [NoiDungBaoTri] nvarchar(500) NOT NULL,
  [MucDoUuTien]   nvarchar(255) NOT NULL,
  [TrangThai]     nvarchar(255) NOT NULL CONSTRAINT [DF_KeHoach_TrangThai] DEFAULT 'ChoThucHien',
  [GhiChu]        nvarchar(500) NULL,
  CONSTRAINT [CK_KeHoach_MucDoUuTien] CHECK ([MucDoUuTien] IN ('Thap', 'TrungBinh', 'Cao')),
  CONSTRAINT [CK_KeHoach_TrangThai] CHECK ([TrangThai] IN ('ChoThucHien', 'DaPhanCong', 'DangThucHien', 'ChoNghiemThu', 'HoanThanh', 'DaHuy'))
)
GO

/* ---------------------------------------------------------------------
   MODULE 4: BaoCaoSuCo, KyThuatVien, PhieuXuLy, PhanCongXuLy
   --------------------------------------------------------------------- */
CREATE TABLE [BaoCaoSuCo] (
  [MaBaoCao]         int PRIMARY KEY IDENTITY(1, 1),
  [MaThietBi]        int NOT NULL,
  [MaTaiKhoanBaoCao] int NOT NULL,
  [NgayBaoCao]       datetime NOT NULL,
  [MoTaSuCo]         nvarchar(500) NOT NULL,
  [MucDoSuCo]        nvarchar(255) NOT NULL,
  [TrangThai]        nvarchar(255) NOT NULL CONSTRAINT [DF_BaoCao_TrangThai] DEFAULT 'MoiBao',
  [GhiChu]           nvarchar(500) NULL,
  CONSTRAINT [CK_BaoCao_MucDo] CHECK ([MucDoSuCo] IN ('Thap', 'TrungBinh', 'Cao', 'KhanCap')),
  CONSTRAINT [CK_BaoCao_TrangThai] CHECK ([TrangThai] IN ('MoiBao', 'DaTiepNhan', 'DangXuLy', 'ChoNghiemThu', 'HoanThanh', 'DaHuy'))
)
GO

CREATE TABLE [KyThuatVien] (
  [MaKyThuatVien] int PRIMARY KEY IDENTITY(1, 1),
  [MaTaiKhoan]    int NULL,
  [HoTen]         nvarchar(100) NOT NULL,
  [ChuyenMon]     nvarchar(150) NULL,
  [SoDienThoai]   nvarchar(15) NULL,
  [Email]         nvarchar(100) NULL,
  [TrangThai]     bit NOT NULL CONSTRAINT [DF_KyThuatVien_TrangThai] DEFAULT (1)
)
GO

CREATE TABLE [PhieuXuLy] (
  [MaPhieuXuLy]   int PRIMARY KEY IDENTITY(1, 1),
  [MaBaoCao]      int NULL,
  [MaKeHoach]     int NULL,
  [LoaiXuLy]      nvarchar(255) NOT NULL,
  [NgayTao]       datetime NOT NULL,
  [NgayBatDau]    datetime NULL,
  [NgayHoanThanh] datetime NULL,
  [TrangThai]     nvarchar(255) NOT NULL CONSTRAINT [DF_Phieu_TrangThai] DEFAULT 'ChoPhanCong',
  [KetQuaXuLy]    nvarchar(500) NULL,
  CONSTRAINT [CK_Phieu_LoaiXuLy] CHECK ([LoaiXuLy] IN ('BaoTriDinhKy', 'XuLySuCo')),
  CONSTRAINT [CK_Phieu_TrangThai] CHECK ([TrangThai] IN ('ChoPhanCong', 'DaPhanCong', 'DangXuLy', 'ChoNghiemThu', 'HoanThanh', 'DaHuy')),
  CONSTRAINT [CK_Phieu_Nguon] CHECK (
       ([LoaiXuLy] = 'XuLySuCo'     AND [MaBaoCao]  IS NOT NULL AND [MaKeHoach] IS NULL)
    OR ([LoaiXuLy] = 'BaoTriDinhKy' AND [MaKeHoach] IS NOT NULL AND [MaBaoCao]  IS NULL))
)
GO

CREATE TABLE [PhanCongXuLy] (
  [MaPhanCong]          int PRIMARY KEY IDENTITY(1, 1),
  [MaPhieuXuLy]         int NOT NULL,
  [MaKyThuatVien]       int NOT NULL,
  [NgayPhanCong]        datetime NOT NULL,
  [NgayKetThucPhanCong] datetime NULL,
  [NoiDungPhanCong]     nvarchar(500) NULL,
  [TrangThai]           nvarchar(255) NOT NULL CONSTRAINT [DF_PhanCong_TrangThai] DEFAULT 'DangHieuLuc',
  CONSTRAINT [CK_PhanCong_TrangThai] CHECK ([TrangThai] IN ('DangHieuLuc', 'DaKetThuc'))
)
GO

/* ---------------------------------------------------------------------
   MODULE 5: VatTu, ChiTietXuLy, TepDinhKem, ThongBao
   --------------------------------------------------------------------- */
CREATE TABLE [VatTu] (
  [MaVatTu]       int PRIMARY KEY IDENTITY(1, 1),
  [TenVatTu]      nvarchar(150) NOT NULL,
  [DonViTinh]     nvarchar(30) NOT NULL,
  [DonGiaMacDinh] decimal(18,2) NOT NULL,
  [SoLuongTon]    int NOT NULL CONSTRAINT [DF_VatTu_Ton] DEFAULT (0),
  [TrangThai]     bit NOT NULL CONSTRAINT [DF_VatTu_TrangThai] DEFAULT (1),
  CONSTRAINT [UQ_VatTu_Ten] UNIQUE ([TenVatTu]),
  CONSTRAINT [CK_VatTu_DonGia] CHECK ([DonGiaMacDinh] >= 0),
  CONSTRAINT [CK_VatTu_Ton] CHECK ([SoLuongTon] >= 0)
)
GO

CREATE TABLE [ChiTietXuLy] (
  [MaChiTiet]       int PRIMARY KEY IDENTITY(1, 1),
  [MaPhieuXuLy]     int NOT NULL,
  [NgayCapNhat]     datetime NOT NULL,
  [NoiDungThucHien] nvarchar(500) NOT NULL,
  [VatTuLinhKien]   nvarchar(150) NULL,
  [MaVatTu]         int NULL,
  [SoLuong]         int NULL,
  [DonGia]          decimal(18,2) NULL,
  [ThanhTien]       decimal(18,2) NULL,
  [GhiChu]          nvarchar(255) NULL,
  CONSTRAINT [CK_ChiTiet_SoLuong] CHECK ([SoLuong] IS NULL OR [SoLuong] > 0),
  CONSTRAINT [CK_ChiTiet_DonGia] CHECK ([DonGia] IS NULL OR [DonGia] >= 0),
  CONSTRAINT [CK_ChiTiet_ThanhTien] CHECK ([ThanhTien] IS NULL
    OR ([SoLuong] IS NOT NULL AND [DonGia] IS NOT NULL AND [ThanhTien] = [SoLuong] * [DonGia]))
)
GO

CREATE TABLE [TepDinhKem] (
  [MaTepDinhKem]     int PRIMARY KEY IDENTITY(1, 1),
  [LoaiDoiTuong]     nvarchar(255) NOT NULL,
  [MaDoiTuong]       int NOT NULL,
  [DuongDanFile]     nvarchar(255) NOT NULL,
  [TenFileGoc]       nvarchar(255) NULL,
  [NgayUpload]       datetime NOT NULL CONSTRAINT [DF_TepDinhKem_Ngay] DEFAULT (getdate()),
  [MaTaiKhoanUpload] int NULL,
  CONSTRAINT [CK_TepDinhKem_Loai] CHECK ([LoaiDoiTuong] IN ('ThietBi', 'BaoCaoSuCo', 'ChiTietXuLy'))
)
GO

CREATE TABLE [ThongBao] (
  [MaThongBao]     int PRIMARY KEY IDENTITY(1, 1),
  [Kenh]           nvarchar(255) NOT NULL,
  [MaTaiKhoanNhan] int NOT NULL,
  [MaThietBi]      int NULL,
  [NoiDung]        nvarchar(500) NOT NULL,
  [NgayTao]        datetime NOT NULL CONSTRAINT [DF_ThongBao_NgayTao] DEFAULT (getdate()),
  [NgayGui]        datetime NULL,
  [TrangThai]      nvarchar(255) NOT NULL CONSTRAINT [DF_ThongBao_TrangThai] DEFAULT 'ChoGui',
  [DaDoc]          bit NOT NULL CONSTRAINT [DF_ThongBao_DaDoc] DEFAULT (0),
  CONSTRAINT [CK_ThongBao_Kenh] CHECK ([Kenh] IN ('Email', 'HeThong')),
  CONSTRAINT [CK_ThongBao_TrangThai] CHECK ([TrangThai] IN ('ChoGui', 'DaGui', 'Loi'))
)
GO

/* ---------------------------------------------------------------------
   KHOA NGOAI (khong ON DELETE CASCADE - bao toan lich su)
   Luu y: KyThuatVien.MaTaiKhoan -> TaiKhoan (KHONG dao chieu)
   --------------------------------------------------------------------- */
ALTER TABLE [TaiKhoan]      ADD CONSTRAINT [FK_TaiKhoan_KhuVuc]       FOREIGN KEY ([MaKhuVuc])         REFERENCES [KhuVuc] ([MaKhuVuc])
ALTER TABLE [NhatKyDangNhap]ADD CONSTRAINT [FK_NhatKy_TaiKhoan]       FOREIGN KEY ([MaTaiKhoan])       REFERENCES [TaiKhoan] ([MaTaiKhoan])
ALTER TABLE [KyThuatVien]   ADD CONSTRAINT [FK_KyThuatVien_TaiKhoan]  FOREIGN KEY ([MaTaiKhoan])       REFERENCES [TaiKhoan] ([MaTaiKhoan])
ALTER TABLE [ThietBi]       ADD CONSTRAINT [FK_ThietBi_Loai]          FOREIGN KEY ([MaLoaiThietBi])    REFERENCES [LoaiThietBi] ([MaLoaiThietBi])
ALTER TABLE [ThietBi]       ADD CONSTRAINT [FK_ThietBi_KhuVuc]        FOREIGN KEY ([MaKhuVuc])         REFERENCES [KhuVuc] ([MaKhuVuc])
ALTER TABLE [KeHoachBaoTri] ADD CONSTRAINT [FK_KeHoach_ThietBi]       FOREIGN KEY ([MaThietBi])        REFERENCES [ThietBi] ([MaThietBi])
ALTER TABLE [BaoCaoSuCo]    ADD CONSTRAINT [FK_BaoCao_ThietBi]        FOREIGN KEY ([MaThietBi])        REFERENCES [ThietBi] ([MaThietBi])
ALTER TABLE [BaoCaoSuCo]    ADD CONSTRAINT [FK_BaoCao_TaiKhoan]       FOREIGN KEY ([MaTaiKhoanBaoCao]) REFERENCES [TaiKhoan] ([MaTaiKhoan])
ALTER TABLE [PhieuXuLy]     ADD CONSTRAINT [FK_Phieu_BaoCao]          FOREIGN KEY ([MaBaoCao])         REFERENCES [BaoCaoSuCo] ([MaBaoCao])
ALTER TABLE [PhieuXuLy]     ADD CONSTRAINT [FK_Phieu_KeHoach]         FOREIGN KEY ([MaKeHoach])        REFERENCES [KeHoachBaoTri] ([MaKeHoach])
ALTER TABLE [PhanCongXuLy]  ADD CONSTRAINT [FK_PhanCong_Phieu]        FOREIGN KEY ([MaPhieuXuLy])      REFERENCES [PhieuXuLy] ([MaPhieuXuLy])
ALTER TABLE [PhanCongXuLy]  ADD CONSTRAINT [FK_PhanCong_KTV]          FOREIGN KEY ([MaKyThuatVien])    REFERENCES [KyThuatVien] ([MaKyThuatVien])
ALTER TABLE [ChiTietXuLy]   ADD CONSTRAINT [FK_ChiTiet_Phieu]         FOREIGN KEY ([MaPhieuXuLy])      REFERENCES [PhieuXuLy] ([MaPhieuXuLy])
ALTER TABLE [ChiTietXuLy]   ADD CONSTRAINT [FK_ChiTiet_VatTu]         FOREIGN KEY ([MaVatTu])          REFERENCES [VatTu] ([MaVatTu])
ALTER TABLE [TepDinhKem]    ADD CONSTRAINT [FK_TepDinhKem_TaiKhoan]   FOREIGN KEY ([MaTaiKhoanUpload]) REFERENCES [TaiKhoan] ([MaTaiKhoan])
ALTER TABLE [ThongBao]      ADD CONSTRAINT [FK_ThongBao_TaiKhoan]     FOREIGN KEY ([MaTaiKhoanNhan])   REFERENCES [TaiKhoan] ([MaTaiKhoan])
ALTER TABLE [ThongBao]      ADD CONSTRAINT [FK_ThongBao_ThietBi]      FOREIGN KEY ([MaThietBi])        REFERENCES [ThietBi] ([MaThietBi])
GO

/* ---------------------------------------------------------------------
   INDEX
   --------------------------------------------------------------------- */
-- UNIQUE tren cot nullable: SQL Server chi cho 1 dong NULL -> dung filtered unique index
CREATE UNIQUE INDEX [UX_KyThuatVien_MaTaiKhoan] ON [KyThuatVien] ([MaTaiKhoan]) WHERE [MaTaiKhoan] IS NOT NULL
-- Moi phieu chi co 1 phan cong dang hieu luc.
-- Khi doi KTV: UPDATE ban ghi cu -> DaKetThuc TRUOC, roi moi INSERT ban ghi moi.
CREATE UNIQUE INDEX [UX_PhanCong_DangHieuLuc]   ON [PhanCongXuLy] ([MaPhieuXuLy]) WHERE [TrangThai] = 'DangHieuLuc'

CREATE INDEX [IX_NhatKy_MaTaiKhoan]  ON [NhatKyDangNhap] ([MaTaiKhoan])
CREATE INDEX [IX_NhatKy_ThoiGian]    ON [NhatKyDangNhap] ([ThoiGian])

CREATE INDEX [IX_ThietBi_Loai]       ON [ThietBi] ([MaLoaiThietBi])
CREATE INDEX [IX_ThietBi_KhuVuc]     ON [ThietBi] ([MaKhuVuc])
CREATE INDEX [IX_ThietBi_TinhTrang]  ON [ThietBi] ([TinhTrang])
CREATE INDEX [IX_ThietBi_Loc]        ON [ThietBi] ([MaLoaiThietBi], [MaKhuVuc], [TinhTrang])

CREATE INDEX [IX_KeHoach_ThietBi]    ON [KeHoachBaoTri] ([MaThietBi])
CREATE INDEX [IX_KeHoach_TrangThai]  ON [KeHoachBaoTri] ([TrangThai])
CREATE INDEX [IX_KeHoach_NgayDuKien] ON [KeHoachBaoTri] ([NgayDuKien])

CREATE INDEX [IX_BaoCao_ThietBi]     ON [BaoCaoSuCo] ([MaThietBi])
CREATE INDEX [IX_BaoCao_TaiKhoan]    ON [BaoCaoSuCo] ([MaTaiKhoanBaoCao])
CREATE INDEX [IX_BaoCao_TrangThai]   ON [BaoCaoSuCo] ([TrangThai])
CREATE INDEX [IX_BaoCao_MucDo]       ON [BaoCaoSuCo] ([MucDoSuCo])

CREATE INDEX [IX_Phieu_BaoCao]       ON [PhieuXuLy] ([MaBaoCao])
CREATE INDEX [IX_Phieu_KeHoach]      ON [PhieuXuLy] ([MaKeHoach])
CREATE INDEX [IX_Phieu_TrangThai]    ON [PhieuXuLy] ([TrangThai])

CREATE INDEX [IX_PhanCong_Phieu]     ON [PhanCongXuLy] ([MaPhieuXuLy])
CREATE INDEX [IX_PhanCong_KTV]       ON [PhanCongXuLy] ([MaKyThuatVien])
CREATE INDEX [IX_PhanCong_TrangThai] ON [PhanCongXuLy] ([TrangThai])
CREATE INDEX [IX_PhanCong_Phieu_TT]  ON [PhanCongXuLy] ([MaPhieuXuLy], [TrangThai])

CREATE INDEX [IX_ChiTiet_Phieu]      ON [ChiTietXuLy] ([MaPhieuXuLy])
CREATE INDEX [IX_ChiTiet_VatTu]      ON [ChiTietXuLy] ([MaVatTu])

CREATE INDEX [IX_TepDinhKem_DoiTuong]ON [TepDinhKem] ([LoaiDoiTuong], [MaDoiTuong])

CREATE INDEX [IX_ThongBao_TaiKhoan]  ON [ThongBao] ([MaTaiKhoanNhan])
CREATE INDEX [IX_ThongBao_TrangThai] ON [ThongBao] ([TrangThai])
GO

/* ---------------------------------------------------------------------
   MO TA (Extended Properties)
   --------------------------------------------------------------------- */
EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'Entity goc cho dang nhap/phan quyen. TenDangNhap khong trung.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TaiKhoan';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Luu ban hash, khong luu plain text',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TaiKhoan', @level2type = N'Column', @level2name = 'MatKhau';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Pham vi khu vuc cua NguoiSuDung. Null voi Admin/KyThuatVien (khong gioi han).',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TaiKhoan', @level2type = N'Column', @level2name = 'MaKhuVuc';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = '1 = Hoat dong, 0 = Bi khoa vinh vien boi Admin',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TaiKhoan', @level2type = N'Column', @level2name = 'TrangThai';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Chi dung neu tu viet ham hash. Neu dung PasswordHasher<T> thi de null.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TaiKhoan', @level2type = N'Column', @level2name = 'MatKhauSalt';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Dem so lan dang nhap sai lien tiep, reset ve 0 khi dang nhap thanh cong',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TaiKhoan', @level2type = N'Column', @level2name = 'SoLanDangNhapSai';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Khoa tam thoi sau N lan sai. Khac TrangThai = 0 (khoa vinh vien)',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TaiKhoan', @level2type = N'Column', @level2name = 'KhoaDenNgay';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'Audit log dang nhap (dung/sai/khoa). MaTaiKhoan null neu nhap sai TenDangNhap. Khong bao gio xoa dong.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'NhatKyDangNhap';

EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Phai > 0',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'LoaiThietBi', @level2type = N'Column', @level2name = 'ChuKyBaoTriMacDinhThang';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'NgayBaoTriTiepTheo KHONG luu cot rieng - tinh dong trong code:
NgayBaoTriTiepTheo = (NgayBaoTriGanNhat ?? NgayDuaVaoSuDung) + ChuKyBaoTriThang (thang)
Chua/Sap/Qua han cung suy ra tu phep so sanh ngay, khong nhap tay.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ThietBi';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = '<= ngay hien tai (kiem tra o Controller/Validation)',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ThietBi', @level2type = N'Column', @level2name = 'NgayDuaVaoSuDung';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Phai > 0',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ThietBi', @level2type = N'Column', @level2name = 'ChuKyBaoTriThang';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Null = chua bao tri lan nao, tinh moc dau tu NgayDuaVaoSuDung',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ThietBi', @level2type = N'Column', @level2name = 'NgayBaoTriGanNhat';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'Khong cho 2 ke hoach dinh ky dang hoat dong cho cung 1 ThietBi + cung ky bao tri (kiem tra bang LINQ, khong phai constraint DB).',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'KeHoachBaoTri';

EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'He thong tu gan, khong cho nhap tay',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'BaoCaoSuCo', @level2type = N'Column', @level2name = 'NgayBaoCao';

EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'FK 0..1 toi TaiKhoan. Unique co dieu kien (WHERE NOT NULL) de nhieu KTV khong co tai khoan',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'KyThuatVien', @level2type = N'Column', @level2name = 'MaTaiKhoan';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = '0 = Ngung hoat dong, khong duoc nhan phan cong moi',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'KyThuatVien', @level2type = N'Column', @level2name = 'TrangThai';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'Dung dung 1 trong 2 FK (MaBaoCao hoac MaKeHoach) theo LoaiXuLy - DB ep bang CHECK CK_Phieu_Nguon, Controller/Service van kiem tra de bao loi than thien.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'PhieuXuLy';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Co gia tri khi LoaiXuLy = XuLySuCo',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'PhieuXuLy', @level2type = N'Column', @level2name = 'MaBaoCao';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Co gia tri khi LoaiXuLy = BaoTriDinhKy',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'PhieuXuLy', @level2type = N'Column', @level2name = 'MaKeHoach';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'Khi doi KTV: KHONG update de ban ghi cu, ma set TrangThai = DaKetThuc + tao ban ghi moi de bao toan lich su. Moi phieu chi 1 ban ghi DangHieuLuc (UX_PhanCong_DangHieuLuc).',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'PhanCongXuLy';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'Danh muc kho vat tu (nang cao). DonGiaMacDinh chi la gia goi y. SoLuongTon tru o Service layer khi luu ChiTietXuLy.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'VatTu';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'TongChiPhi cua 1 PhieuXuLy = SUM(ThanhTien) cac ChiTietXuLy - tinh dong bang LINQ, khong luu cot rieng o PhieuXuLy.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ChiTietXuLy';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Ten vat tu luu vet (snapshot). Neu chon tu kho thi copy TenVatTu vao day tai thoi diem dung',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ChiTietXuLy', @level2type = N'Column', @level2name = 'VatTuLinhKien';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'FK toi VatTu (nang cao), null neu nhap tay ad-hoc',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ChiTietXuLy', @level2type = N'Column', @level2name = 'MaVatTu';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Phai > 0 neu co vat tu',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ChiTietXuLy', @level2type = N'Column', @level2name = 'SoLuong';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'Phai >= 0',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ChiTietXuLy', @level2type = N'Column', @level2name = 'DonGia';
EXEC sp_addextendedproperty @name = N'Column_Description',
  @value = 'He thong tinh = SoLuong * DonGia, khong cho nhap tay',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ChiTietXuLy', @level2type = N'Column', @level2name = 'ThanhTien';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'Tep dinh kem dung chung (nang cao). MaDoiTuong la FK da hinh, khong co FK that - kiem tra o Service layer.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TepDinhKem';

EXEC sp_addextendedproperty @name = N'Table_Description',
  @value = 'Thong bao Email/HeThong (nang cao). Ghi log truoc khi gui de tranh gui trung.',
  @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = 'ThongBao';
GO