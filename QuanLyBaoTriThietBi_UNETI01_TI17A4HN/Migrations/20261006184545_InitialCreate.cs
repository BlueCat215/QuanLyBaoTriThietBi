using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhuVuc",
                columns: table => new
                {
                    MaKhuVuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhuVuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ViTri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NguoiPhuTrach = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuVuc", x => x.MaKhuVuc);
                });

            migrationBuilder.CreateTable(
                name: "LoaiThietBi",
                columns: table => new
                {
                    MaLoaiThietBi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiThietBi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChuKyBaoTriMacDinhThang = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiThietBi", x => x.MaLoaiThietBi);
                    table.CheckConstraint("CK_LoaiThietBi_ChuKy", "[ChuKyBaoTriMacDinhThang] > 0");
                });

            migrationBuilder.CreateTable(
                name: "VatTu",
                columns: table => new
                {
                    MaVatTu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVatTu = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DonGiaMacDinh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatTu", x => x.MaVatTu);
                    table.CheckConstraint("CK_VatTu_DonGia", "[DonGiaMacDinh] >= 0");
                    table.CheckConstraint("CK_VatTu_Ton", "[SoLuongTon] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaKhuVuc = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    MatKhauSalt = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SoLanDangNhapSai = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    KhoaDenNgay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayTaoTaiKhoan = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    LanDangNhapCuoi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                    table.CheckConstraint("CK_TaiKhoan_SoLanSai", "[SoLanDangNhapSai] >= 0");
                    table.CheckConstraint("CK_TaiKhoan_VaiTro", "[VaiTro] IN ('Admin', 'KyThuatVien', 'NguoiSuDung')");
                    table.ForeignKey(
                        name: "FK_TaiKhoan_KhuVuc_MaKhuVuc",
                        column: x => x.MaKhuVuc,
                        principalTable: "KhuVuc",
                        principalColumn: "MaKhuVuc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThietBi",
                columns: table => new
                {
                    MaThietBi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThietBi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaLoaiThietBi = table.Column<int>(type: "int", nullable: false),
                    MaKhuVuc = table.Column<int>(type: "int", nullable: false),
                    SoSerial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayDuaVaoSuDung = table.Column<DateTime>(type: "date", nullable: false),
                    ChuKyBaoTriThang = table.Column<int>(type: "int", nullable: false),
                    NgayBaoTriGanNhat = table.Column<DateTime>(type: "date", nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietBi", x => x.MaThietBi);
                    table.CheckConstraint("CK_ThietBi_ChuKy", "[ChuKyBaoTriThang] > 0");
                    table.CheckConstraint("CK_ThietBi_TinhTrang", "[TinhTrang] IN ('DangHoatDong', 'DangBaoTri', 'TamNgungDoSuCo', 'NgungSuDung')");
                    table.ForeignKey(
                        name: "FK_ThietBi_KhuVuc_MaKhuVuc",
                        column: x => x.MaKhuVuc,
                        principalTable: "KhuVuc",
                        principalColumn: "MaKhuVuc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThietBi_LoaiThietBi_MaLoaiThietBi",
                        column: x => x.MaLoaiThietBi,
                        principalTable: "LoaiThietBi",
                        principalColumn: "MaLoaiThietBi",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KyThuatVien",
                columns: table => new
                {
                    MaKyThuatVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChuyenMon = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyThuatVien", x => x.MaKyThuatVien);
                    table.ForeignKey(
                        name: "FK_KyThuatVien_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyDangNhap",
                columns: table => new
                {
                    MaNhatKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    TenDangNhapNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    KetQua = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DiaChiIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyDangNhap", x => x.MaNhatKy);
                    table.CheckConstraint("CK_NhatKy_KetQua", "[KetQua] IN ('ThanhCong', 'SaiMatKhau', 'TaiKhoanBiKhoa', 'KhongTonTaiTaiKhoan')");
                    table.ForeignKey(
                        name: "FK_NhatKyDangNhap_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TepDinhKem",
                columns: table => new
                {
                    MaTepDinhKem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiDoiTuong = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaDoiTuong = table.Column<int>(type: "int", nullable: false),
                    DuongDanFile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenFileGoc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NgayUpload = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    MaTaiKhoanUpload = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepDinhKem", x => x.MaTepDinhKem);
                    table.CheckConstraint("CK_TepDinhKem_Loai", "[LoaiDoiTuong] IN ('ThietBi', 'BaoCaoSuCo', 'ChiTietXuLy')");
                    table.ForeignKey(
                        name: "FK_TepDinhKem_TaiKhoan_MaTaiKhoanUpload",
                        column: x => x.MaTaiKhoanUpload,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoSuCo",
                columns: table => new
                {
                    MaBaoCao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThietBi = table.Column<int>(type: "int", nullable: false),
                    MaTaiKhoanBaoCao = table.Column<int>(type: "int", nullable: false),
                    NgayBaoCao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoTaSuCo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MucDoSuCo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "MoiBao"),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoSuCo", x => x.MaBaoCao);
                    table.CheckConstraint("CK_BaoCao_MucDo", "[MucDoSuCo] IN ('Thap', 'TrungBinh', 'Cao', 'KhanCap')");
                    table.CheckConstraint("CK_BaoCao_TrangThai", "[TrangThai] IN ('MoiBao', 'DaTiepNhan', 'DangXuLy', 'ChoNghiemThu', 'HoanThanh', 'DaHuy')");
                    table.ForeignKey(
                        name: "FK_BaoCaoSuCo_TaiKhoan_MaTaiKhoanBaoCao",
                        column: x => x.MaTaiKhoanBaoCao,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaoCaoSuCo_ThietBi_MaThietBi",
                        column: x => x.MaThietBi,
                        principalTable: "ThietBi",
                        principalColumn: "MaThietBi",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KeHoachBaoTri",
                columns: table => new
                {
                    MaKeHoach = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThietBi = table.Column<int>(type: "int", nullable: false),
                    NgayDuKien = table.Column<DateTime>(type: "date", nullable: false),
                    NoiDungBaoTri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MucDoUuTien = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "ChoThucHien"),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeHoachBaoTri", x => x.MaKeHoach);
                    table.CheckConstraint("CK_KeHoach_MucDoUuTien", "[MucDoUuTien] IN ('Thap', 'TrungBinh', 'Cao')");
                    table.CheckConstraint("CK_KeHoach_TrangThai", "[TrangThai] IN ('ChoThucHien', 'DaPhanCong', 'DangThucHien', 'ChoNghiemThu', 'HoanThanh', 'DaHuy')");
                    table.ForeignKey(
                        name: "FK_KeHoachBaoTri_ThietBi_MaThietBi",
                        column: x => x.MaThietBi,
                        principalTable: "ThietBi",
                        principalColumn: "MaThietBi",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    MaThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kenh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaTaiKhoanNhan = table.Column<int>(type: "int", nullable: false),
                    MaThietBi = table.Column<int>(type: "int", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "ChoGui"),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.MaThongBao);
                    table.CheckConstraint("CK_ThongBao_Kenh", "[Kenh] IN ('Email', 'HeThong')");
                    table.CheckConstraint("CK_ThongBao_TrangThai", "[TrangThai] IN ('ChoGui', 'DaGui', 'Loi')");
                    table.ForeignKey(
                        name: "FK_ThongBao_TaiKhoan_MaTaiKhoanNhan",
                        column: x => x.MaTaiKhoanNhan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThongBao_ThietBi_MaThietBi",
                        column: x => x.MaThietBi,
                        principalTable: "ThietBi",
                        principalColumn: "MaThietBi",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuXuLy",
                columns: table => new
                {
                    MaPhieuXuLy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBaoCao = table.Column<int>(type: "int", nullable: true),
                    MaKeHoach = table.Column<int>(type: "int", nullable: true),
                    LoaiXuLy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayHoanThanh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "ChoPhanCong"),
                    KetQuaXuLy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuXuLy", x => x.MaPhieuXuLy);
                    table.CheckConstraint("CK_Phieu_LoaiXuLy", "[LoaiXuLy] IN ('BaoTriDinhKy', 'XuLySuCo')");
                    table.CheckConstraint("CK_Phieu_Nguon", "([LoaiXuLy] = 'XuLySuCo' AND [MaBaoCao] IS NOT NULL AND [MaKeHoach] IS NULL) OR ([LoaiXuLy] = 'BaoTriDinhKy' AND [MaKeHoach] IS NOT NULL AND [MaBaoCao] IS NULL)");
                    table.CheckConstraint("CK_Phieu_TrangThai", "[TrangThai] IN ('ChoPhanCong', 'DaPhanCong', 'DangXuLy', 'ChoNghiemThu', 'HoanThanh', 'DaHuy')");
                    table.ForeignKey(
                        name: "FK_PhieuXuLy_BaoCaoSuCo_MaBaoCao",
                        column: x => x.MaBaoCao,
                        principalTable: "BaoCaoSuCo",
                        principalColumn: "MaBaoCao",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuXuLy_KeHoachBaoTri_MaKeHoach",
                        column: x => x.MaKeHoach,
                        principalTable: "KeHoachBaoTri",
                        principalColumn: "MaKeHoach",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietXuLy",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieuXuLy = table.Column<int>(type: "int", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoiDungThucHien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VatTuLinhKien = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MaVatTu = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietXuLy", x => x.MaChiTiet);
                    table.CheckConstraint("CK_ChiTiet_DonGia", "[DonGia] IS NULL OR [DonGia] >= 0");
                    table.CheckConstraint("CK_ChiTiet_SoLuong", "[SoLuong] IS NULL OR [SoLuong] > 0");
                    table.CheckConstraint("CK_ChiTiet_ThanhTien", "[ThanhTien] IS NULL OR ([SoLuong] IS NOT NULL AND [DonGia] IS NOT NULL AND [ThanhTien] = [SoLuong] * [DonGia])");
                    table.ForeignKey(
                        name: "FK_ChiTietXuLy_PhieuXuLy_MaPhieuXuLy",
                        column: x => x.MaPhieuXuLy,
                        principalTable: "PhieuXuLy",
                        principalColumn: "MaPhieuXuLy",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietXuLy_VatTu_MaVatTu",
                        column: x => x.MaVatTu,
                        principalTable: "VatTu",
                        principalColumn: "MaVatTu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanCongXuLy",
                columns: table => new
                {
                    MaPhanCong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieuXuLy = table.Column<int>(type: "int", nullable: false),
                    MaKyThuatVien = table.Column<int>(type: "int", nullable: false),
                    NgayPhanCong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThucPhanCong = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoiDungPhanCong = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "DangHieuLuc")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongXuLy", x => x.MaPhanCong);
                    table.CheckConstraint("CK_PhanCong_TrangThai", "[TrangThai] IN ('DangHieuLuc', 'DaKetThuc')");
                    table.ForeignKey(
                        name: "FK_PhanCongXuLy_KyThuatVien_MaKyThuatVien",
                        column: x => x.MaKyThuatVien,
                        principalTable: "KyThuatVien",
                        principalColumn: "MaKyThuatVien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongXuLy_PhieuXuLy_MaPhieuXuLy",
                        column: x => x.MaPhieuXuLy,
                        principalTable: "PhieuXuLy",
                        principalColumn: "MaPhieuXuLy",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_MucDo",
                table: "BaoCaoSuCo",
                column: "MucDoSuCo");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_TaiKhoan",
                table: "BaoCaoSuCo",
                column: "MaTaiKhoanBaoCao");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_ThietBi",
                table: "BaoCaoSuCo",
                column: "MaThietBi");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_TrangThai",
                table: "BaoCaoSuCo",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTiet_Phieu",
                table: "ChiTietXuLy",
                column: "MaPhieuXuLy");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTiet_VatTu",
                table: "ChiTietXuLy",
                column: "MaVatTu");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoach_NgayDuKien",
                table: "KeHoachBaoTri",
                column: "NgayDuKien");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoach_ThietBi",
                table: "KeHoachBaoTri",
                column: "MaThietBi");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoach_TrangThai",
                table: "KeHoachBaoTri",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "UQ_KhuVuc_Ten",
                table: "KhuVuc",
                column: "TenKhuVuc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_KyThuatVien_MaTaiKhoan",
                table: "KyThuatVien",
                column: "MaTaiKhoan",
                unique: true,
                filter: "[MaTaiKhoan] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_LoaiThietBi_Ten",
                table: "LoaiThietBi",
                column: "TenLoaiThietBi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhatKy_MaTaiKhoan",
                table: "NhatKyDangNhap",
                column: "MaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKy_ThoiGian",
                table: "NhatKyDangNhap",
                column: "ThoiGian");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_KTV",
                table: "PhanCongXuLy",
                column: "MaKyThuatVien");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_Phieu_TT",
                table: "PhanCongXuLy",
                columns: new[] { "MaPhieuXuLy", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_TrangThai",
                table: "PhanCongXuLy",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "UX_PhanCong_DangHieuLuc",
                table: "PhanCongXuLy",
                column: "MaPhieuXuLy",
                unique: true,
                filter: "[TrangThai] = 'DangHieuLuc'");

            migrationBuilder.CreateIndex(
                name: "IX_Phieu_BaoCao",
                table: "PhieuXuLy",
                column: "MaBaoCao");

            migrationBuilder.CreateIndex(
                name: "IX_Phieu_KeHoach",
                table: "PhieuXuLy",
                column: "MaKeHoach");

            migrationBuilder.CreateIndex(
                name: "IX_Phieu_TrangThai",
                table: "PhieuXuLy",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaKhuVuc",
                table: "TaiKhoan",
                column: "MaKhuVuc");

            migrationBuilder.CreateIndex(
                name: "UQ_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_DoiTuong",
                table: "TepDinhKem",
                columns: new[] { "LoaiDoiTuong", "MaDoiTuong" });

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_MaTaiKhoanUpload",
                table: "TepDinhKem",
                column: "MaTaiKhoanUpload");

            migrationBuilder.CreateIndex(
                name: "IX_ThietBi_KhuVuc",
                table: "ThietBi",
                column: "MaKhuVuc");

            migrationBuilder.CreateIndex(
                name: "IX_ThietBi_Loai",
                table: "ThietBi",
                column: "MaLoaiThietBi");

            migrationBuilder.CreateIndex(
                name: "IX_ThietBi_Loc",
                table: "ThietBi",
                columns: new[] { "MaLoaiThietBi", "MaKhuVuc", "TinhTrang" });

            migrationBuilder.CreateIndex(
                name: "IX_ThietBi_TinhTrang",
                table: "ThietBi",
                column: "TinhTrang");

            migrationBuilder.CreateIndex(
                name: "UQ_ThietBi_SoSerial",
                table: "ThietBi",
                column: "SoSerial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaThietBi",
                table: "ThongBao",
                column: "MaThietBi");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_TaiKhoan",
                table: "ThongBao",
                column: "MaTaiKhoanNhan");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_TrangThai",
                table: "ThongBao",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "UQ_VatTu_Ten",
                table: "VatTu",
                column: "TenVatTu",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietXuLy");

            migrationBuilder.DropTable(
                name: "NhatKyDangNhap");

            migrationBuilder.DropTable(
                name: "PhanCongXuLy");

            migrationBuilder.DropTable(
                name: "TepDinhKem");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "VatTu");

            migrationBuilder.DropTable(
                name: "KyThuatVien");

            migrationBuilder.DropTable(
                name: "PhieuXuLy");

            migrationBuilder.DropTable(
                name: "BaoCaoSuCo");

            migrationBuilder.DropTable(
                name: "KeHoachBaoTri");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "ThietBi");

            migrationBuilder.DropTable(
                name: "KhuVuc");

            migrationBuilder.DropTable(
                name: "LoaiThietBi");
        }
    }
}
