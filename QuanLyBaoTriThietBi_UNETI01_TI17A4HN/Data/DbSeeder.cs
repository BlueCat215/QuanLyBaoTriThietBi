#nullable enable
using Microsoft.AspNetCore.Identity;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;

/// Seed du lieu mau (chi chay khi bang TaiKhoan con trong).
/// Tat ca tai khoan co mat khau: 123456 (da hash bang PasswordHasher).
/// Ngay tham chieu cua du lieu mau: 2026-10-03.

public static class DbSeeder
{
    private const string MatKhauMau = "123456";

    public static void Seed(AppDbContext db)
    {
        if (db.TaiKhoans.Any()) return;

        var hasher = new PasswordHasher<TaiKhoan>();

        TaiKhoan Tk(string user, string hoTen, string email, VaiTroEnum vaiTro, KhuVuc? kv = null, bool hoatDong = true)
        {
            var t = new TaiKhoan
            {
                TenDangNhap = user,
                HoTen = hoTen,
                Email = email,
                VaiTro = vaiTro,
                KhuVuc = kv,
                TrangThai = hoatDong,
                NgayTaoTaiKhoan = new DateTime(2026, 1, 5, 8, 0, 0)
            };
            t.MatKhau = hasher.HashPassword(t, MatKhauMau);
            return t;
        }

        // ------------------------------------------------ KhuVuc
        var kv1 = new KhuVuc { TenKhuVuc = "Văn phòng hành chính", ViTri = "Tòa A - Tầng 1", NguoiPhuTrach = "Vũ Thị Lan", MoTa = "Khu làm việc hành chính - nhân sự" };
        var kv2 = new KhuVuc { TenKhuVuc = "Phòng họp lớn", ViTri = "Tòa A - Tầng 2", NguoiPhuTrach = "Hoàng Văn Nam", MoTa = "Phòng họp 40 chỗ" };
        var kv3 = new KhuVuc { TenKhuVuc = "Phòng kế toán", ViTri = "Tòa A - Tầng 3", NguoiPhuTrach = "Đặng Thị Mai", MoTa = "Khu làm việc tài chính - kế toán" };
        var kv4 = new KhuVuc { TenKhuVuc = "Phòng máy chủ", ViTri = "Tòa B - Tầng 1", NguoiPhuTrach = "Trần Minh Tuấn", MoTa = "Phòng server, thiết bị mạng lõi" };
        var kv5 = new KhuVuc { TenKhuVuc = "Kho thiết bị cũ", ViTri = "Tòa B - Tầng hầm", NguoiPhuTrach = "Phạm Quốc Bảo", MoTa = "Lưu trữ thiết bị ngưng sử dụng" };
        var khuVucs = new[] { kv1, kv2, kv3, kv4, kv5 };

        // ------------------------------------------------ LoaiThietBi
        var lPc = new LoaiThietBi { TenLoaiThietBi = "Máy tính để bàn", ChuKyBaoTriMacDinhThang = 6, MoTa = "PC văn phòng, PC phòng thực hành" };
        var lPr = new LoaiThietBi { TenLoaiThietBi = "Máy in", ChuKyBaoTriMacDinhThang = 6, MoTa = "Máy in laser, máy in đa năng" };
        var lAc = new LoaiThietBi { TenLoaiThietBi = "Điều hòa", ChuKyBaoTriMacDinhThang = 3, MoTa = "Điều hòa không khí treo tường" };
        var lPj = new LoaiThietBi { TenLoaiThietBi = "Máy chiếu", ChuKyBaoTriMacDinhThang = 6, MoTa = "Máy chiếu phòng họp, phòng học" };
        var lNw = new LoaiThietBi { TenLoaiThietBi = "Thiết bị mạng", ChuKyBaoTriMacDinhThang = 12, MoTa = "Switch, router, access point" };
        var lUp = new LoaiThietBi { TenLoaiThietBi = "Bộ lưu điện UPS", ChuKyBaoTriMacDinhThang = 6, MoTa = "UPS cho phòng máy chủ" };
        var loais = new[] { lPc, lPr, lAc, lPj, lNw, lUp };

        // ------------------------------------------------ TaiKhoan (NguoiSuDung gan KhuVuc)
        var admin = Tk("admin", "Nguyễn Quản Trị", "admin@congty.vn", VaiTroEnum.Admin);
        var tkHung = Tk("ktv_hung", "Nguyễn Văn Hùng", "hung.nv@congty.vn", VaiTroEnum.KyThuatVien);
        var tkTuan = Tk("ktv_tuan", "Trần Minh Tuấn", "tuan.tm@congty.vn", VaiTroEnum.KyThuatVien);
        var tkHoa = Tk("ktv_hoa", "Lê Thị Hoa", "hoa.lt@congty.vn", VaiTroEnum.KyThuatVien);
        var tkBao = Tk("ktv_bao", "Phạm Quốc Bảo", "bao.pq@congty.vn", VaiTroEnum.KyThuatVien);
        var uLan = Tk("user_lan", "Vũ Thị Lan", "lan.vt@congty.vn", VaiTroEnum.NguoiSuDung, kv1);
        var uNam = Tk("user_nam", "Hoàng Văn Nam", "nam.hv@congty.vn", VaiTroEnum.NguoiSuDung, kv2);
        var uMai = Tk("user_mai", "Đặng Thị Mai", "mai.dt@congty.vn", VaiTroEnum.NguoiSuDung, kv3);
        var uKhoa = Tk("user_khoa", "Bùi Anh Khoa", "khoa.ba@congty.vn", VaiTroEnum.NguoiSuDung, kv4);
        var uCu = Tk("user_cu", "Ngô Văn Cũ (đã nghỉ)", "cu.nv@congty.vn", VaiTroEnum.NguoiSuDung, kv1, hoatDong: false);
        var taiKhoans = new[] { admin, tkHung, tkTuan, tkHoa, tkBao, uLan, uNam, uMai, uKhoa, uCu };

        // ------------------------------------------------ KyThuatVien (KTV5: khong co tai khoan, da ngung)
        var ktv1 = new KyThuatVien { TaiKhoan = tkHung, HoTen = "Nguyễn Văn Hùng", ChuyenMon = "Điện lạnh, máy chiếu", SoDienThoai = "0901234567", Email = "hung.nv@congty.vn" };
        var ktv2 = new KyThuatVien { TaiKhoan = tkTuan, HoTen = "Trần Minh Tuấn", ChuyenMon = "Mạng và hệ thống", SoDienThoai = "0912345678", Email = "tuan.tm@congty.vn" };
        var ktv3 = new KyThuatVien { TaiKhoan = tkHoa, HoTen = "Lê Thị Hoa", ChuyenMon = "Phần cứng máy tính", SoDienThoai = "0923456789", Email = "hoa.lt@congty.vn" };
        var ktv4 = new KyThuatVien { TaiKhoan = tkBao, HoTen = "Phạm Quốc Bảo", ChuyenMon = "Máy in, thiết bị văn phòng", SoDienThoai = "0934567890", Email = "bao.pq@congty.vn" };
        var ktv5 = new KyThuatVien { TaiKhoan = null, HoTen = "Đỗ Văn Long", ChuyenMon = "Điện - UPS", SoDienThoai = "0945678901", TrangThai = false };
        var ktvs = new[] { ktv1, ktv2, ktv3, ktv4, ktv5 };

        // ------------------------------------------------ ThietBi
        ThietBi Tb(string ten, LoaiThietBi loai, KhuVuc kv, string serial, DateTime ngaySd, int chuKy,
                   DateTime? gan, TinhTrangThietBiEnum tt, string? moTa = null) => new()
        {
            TenThietBi = ten, LoaiThietBi = loai, KhuVuc = kv, SoSerial = serial,
            NgayDuaVaoSuDung = ngaySd, ChuKyBaoTriThang = chuKy, NgayBaoTriGanNhat = gan, TinhTrang = tt, MoTa = moTa
        };

        var tb1 = Tb("Máy tính Dell OptiPlex 7090", lPc, kv1, "SN-PC-0001", new(2023, 3, 15), 6, new(2026, 4, 10), TinhTrangThietBiEnum.DangHoatDong, "Core i5, RAM 16GB");
        var tb2 = Tb("Máy tính HP ProDesk 400 G7", lPc, kv2, "SN-PC-0002", new(2023, 6, 1), 6, new(2026, 8, 13), TinhTrangThietBiEnum.DangHoatDong, "Core i5, RAM 8GB");
        var tb3 = Tb("Máy in HP LaserJet Pro M404dn", lPr, kv1, "SN-PR-0001", new(2022, 9, 10), 6, new(2026, 2, 15), TinhTrangThietBiEnum.DangHoatDong, "In 2 mặt, mạng LAN");
        var tb4 = Tb("Máy in Canon LBP6030", lPr, kv3, "SN-PR-0002", new(2021, 5, 20), 6, null, TinhTrangThietBiEnum.TamNgungDoSuCo, "Đang chờ sửa kẹt giấy");
        var tb5 = Tb("Điều hòa Daikin 18000BTU", lAc, kv1, "SN-AC-0001", new(2022, 4, 1), 3, new(2026, 7, 5), TinhTrangThietBiEnum.DangHoatDong);
        var tb6 = Tb("Điều hòa Panasonic 24000BTU", lAc, kv4, "SN-AC-0002", new(2021, 8, 12), 3, new(2026, 6, 20), TinhTrangThietBiEnum.DangBaoTri, "Điều hòa phòng máy chủ");
        var tb7 = Tb("Máy chiếu Epson EB-X49", lPj, kv2, "SN-PJ-0001", new(2023, 1, 10), 6, new(2026, 1, 18), TinhTrangThietBiEnum.DangHoatDong);
        var tb8 = Tb("Máy chiếu Sony VPL-DX221", lPj, kv5, "SN-PJ-0002", new(2020, 11, 5), 6, new(2025, 12, 1), TinhTrangThietBiEnum.NgungSuDung, "Hỏng bóng đèn, đã thanh lý");
        var tb9 = Tb("Switch Cisco Catalyst 2960-24", lNw, kv4, "SN-NW-0001", new(2022, 2, 14), 12, new(2025, 10, 15), TinhTrangThietBiEnum.DangHoatDong, "Switch lõi tầng 1");
        var tb10 = Tb("Router MikroTik RB4011", lNw, kv4, "SN-NW-0002", new(2024, 1, 8), 12, null, TinhTrangThietBiEnum.DangHoatDong, "Router biên");
        var tb11 = Tb("UPS APC Smart-UPS 1500VA", lUp, kv4, "SN-UP-0001", new(2023, 9, 1), 6, new(2026, 3, 2), TinhTrangThietBiEnum.DangHoatDong);
        var tb12 = Tb("Máy tính Lenovo ThinkCentre M70", lPc, kv3, "SN-PC-0003", new(2024, 5, 6), 6, new(2026, 5, 12), TinhTrangThietBiEnum.DangHoatDong, "Máy kế toán trưởng");
        var thietBis = new[] { tb1, tb2, tb3, tb4, tb5, tb6, tb7, tb8, tb9, tb10, tb11, tb12 };

        // ------------------------------------------------ KeHoachBaoTri
        KeHoachBaoTri Kh(ThietBi tb, DateTime ngay, string nd, MucDoUuTienEnum uuTien, TrangThaiKeHoachEnum tt, string? ghiChu = null) => new()
        {
            ThietBi = tb, NgayDuKien = ngay, NoiDungBaoTri = nd, MucDoUuTien = uuTien, TrangThai = tt, GhiChu = ghiChu
        };

        var kh1 = Kh(tb5, new(2026, 7, 5), "Vệ sinh dàn nóng/lạnh, kiểm tra gas", MucDoUuTienEnum.TrungBinh, TrangThaiKeHoachEnum.HoanThanh);
        var kh2 = Kh(tb6, new(2026, 10, 5), "Vệ sinh, kiểm tra block và tụ khởi động", MucDoUuTienEnum.Cao, TrangThaiKeHoachEnum.DangThucHien, "Điều hòa phòng máy chủ, ưu tiên cao");
        var kh3 = Kh(tb9, new(2026, 10, 15), "Kiểm tra firmware, cổng kết nối, quạt tản nhiệt", MucDoUuTienEnum.TrungBinh, TrangThaiKeHoachEnum.ChoThucHien);
        var kh4 = Kh(tb10, new(2026, 10, 10), "Backup cấu hình, cập nhật firmware", MucDoUuTienEnum.Cao, TrangThaiKeHoachEnum.DaPhanCong, "Thực hiện ngoài giờ hành chính");
        var kh5 = Kh(tb1, new(2026, 4, 10), "Vệ sinh case, kiểm tra ổ cứng, tra keo tản nhiệt", MucDoUuTienEnum.Thap, TrangThaiKeHoachEnum.HoanThanh);
        var kh6 = Kh(tb7, new(2026, 7, 18), "Vệ sinh lọc bụi, kiểm tra bóng đèn", MucDoUuTienEnum.TrungBinh, TrangThaiKeHoachEnum.ChoNghiemThu);
        var kh7 = Kh(tb3, new(2026, 8, 15), "Bảo trì định kỳ máy in", MucDoUuTienEnum.Thap, TrangThaiKeHoachEnum.DaHuy, "Hủy do trùng lịch, sẽ lập lại kế hoạch mới");
        var kh8 = Kh(tb11, new(2026, 9, 2), "Kiểm tra ắc quy, tải UPS", MucDoUuTienEnum.TrungBinh, TrangThaiKeHoachEnum.ChoThucHien, "Đã quá hạn, cần xếp lịch");
        var keHoachs = new[] { kh1, kh2, kh3, kh4, kh5, kh6, kh7, kh8 };

        // ------------------------------------------------ BaoCaoSuCo (nguoi bao cao thuoc dung khu vuc cua thiet bi)
        BaoCaoSuCo Bc(ThietBi tb, TaiKhoan nguoi, DateTime ngay, string mota, MucDoSuCoEnum mucDo, TrangThaiBaoCaoEnum tt, string? ghiChu = null) => new()
        {
            ThietBi = tb, TaiKhoanBaoCao = nguoi, NgayBaoCao = ngay, MoTaSuCo = mota, MucDoSuCo = mucDo, TrangThai = tt, GhiChu = ghiChu
        };

        var bc1 = Bc(tb4, uMai, new(2026, 9, 20, 8, 30, 0), "Máy in kẹt giấy liên tục, không in được", MucDoSuCoEnum.Cao, TrangThaiBaoCaoEnum.DangXuLy);
        var bc2 = Bc(tb2, uNam, new(2026, 8, 12, 14, 10, 0), "Máy tính khởi động chậm, thường xuyên treo", MucDoSuCoEnum.TrungBinh, TrangThaiBaoCaoEnum.HoanThanh);
        var bc3 = Bc(tb12, uMai, new(2026, 9, 28, 9, 45, 0), "Màn hình không lên hình sau khi bật nguồn", MucDoSuCoEnum.Cao, TrangThaiBaoCaoEnum.MoiBao);
        var bc4 = Bc(tb9, uKhoa, new(2026, 9, 15, 10, 20, 0), "Mạng chập chờn khu vực phòng máy chủ", MucDoSuCoEnum.KhanCap, TrangThaiBaoCaoEnum.ChoNghiemThu, "Ảnh hưởng toàn bộ hệ thống nội bộ");
        var bc5 = Bc(tb5, uLan, new(2026, 7, 22, 16, 0, 0), "Điều hòa chảy nước", MucDoSuCoEnum.Thap, TrangThaiBaoCaoEnum.DaHuy, "Trùng báo cáo, đã xử lý trong bảo trì định kỳ");
        var bc6 = Bc(tb3, uLan, new(2026, 10, 1, 11, 5, 0), "Máy in bản in bị mờ chữ", MucDoSuCoEnum.Thap, TrangThaiBaoCaoEnum.DaTiepNhan);
        var bc7 = Bc(tb1, uLan, new(2026, 5, 3, 13, 30, 0), "Không nhận chuột và bàn phím USB", MucDoSuCoEnum.Thap, TrangThaiBaoCaoEnum.HoanThanh);
        var baoCaos = new[] { bc1, bc2, bc3, bc4, bc5, bc6, bc7 };

        // ------------------------------------------------ PhieuXuLy (dung 1 trong 2 nguon theo LoaiXuLy)
        PhieuXuLy PhieuDinhKy(KeHoachBaoTri kh, DateTime tao, DateTime? bd, DateTime? ht, TrangThaiPhieuXuLyEnum tt, string? ketQua = null) => new()
        {
            KeHoachBaoTri = kh, LoaiXuLy = LoaiXuLyEnum.BaoTriDinhKy, NgayTao = tao, NgayBatDau = bd, NgayHoanThanh = ht, TrangThai = tt, KetQuaXuLy = ketQua
        };
        PhieuXuLy PhieuSuCo(BaoCaoSuCo bc, DateTime tao, DateTime? bd, DateTime? ht, TrangThaiPhieuXuLyEnum tt, string? ketQua = null) => new()
        {
            BaoCaoSuCo = bc, LoaiXuLy = LoaiXuLyEnum.XuLySuCo, NgayTao = tao, NgayBatDau = bd, NgayHoanThanh = ht, TrangThai = tt, KetQuaXuLy = ketQua
        };

        var p1 = PhieuDinhKy(kh1, new(2026, 6, 30, 9, 0, 0), new(2026, 7, 5, 8, 0, 0), new(2026, 7, 5, 11, 30, 0), TrangThaiPhieuXuLyEnum.HoanThanh, "Đã vệ sinh và nạp bổ sung gas, máy chạy ổn định");
        var p2 = PhieuDinhKy(kh2, new(2026, 10, 1, 9, 0, 0), new(2026, 10, 2, 8, 30, 0), null, TrangThaiPhieuXuLyEnum.DangXuLy);
        var p3 = PhieuDinhKy(kh4, new(2026, 10, 2, 10, 0, 0), null, null, TrangThaiPhieuXuLyEnum.DaPhanCong);
        var p4 = PhieuDinhKy(kh6, new(2026, 7, 10, 9, 0, 0), new(2026, 7, 18, 14, 0, 0), new(2026, 7, 18, 15, 30, 0), TrangThaiPhieuXuLyEnum.ChoNghiemThu, "Đã vệ sinh lọc bụi, bóng đèn còn tốt, chờ nghiệm thu");
        var p5 = PhieuDinhKy(kh5, new(2026, 4, 5, 9, 0, 0), new(2026, 4, 10, 9, 0, 0), new(2026, 4, 10, 10, 30, 0), TrangThaiPhieuXuLyEnum.HoanThanh, "Đã vệ sinh, tra keo tản nhiệt, ổ cứng tốt");
        var p6 = PhieuSuCo(bc1, new(2026, 9, 20, 9, 0, 0), new(2026, 9, 21, 9, 0, 0), null, TrangThaiPhieuXuLyEnum.DangXuLy);
        var p7 = PhieuSuCo(bc2, new(2026, 8, 12, 15, 0, 0), new(2026, 8, 13, 8, 30, 0), new(2026, 8, 13, 11, 0, 0), TrangThaiPhieuXuLyEnum.HoanThanh, "Cài lại hệ điều hành, nâng cấp SSD, máy hoạt động tốt");
        var p8 = PhieuSuCo(bc4, new(2026, 9, 15, 10, 40, 0), new(2026, 9, 15, 11, 0, 0), new(2026, 9, 16, 9, 0, 0), TrangThaiPhieuXuLyEnum.ChoNghiemThu, "Thay quạt tản nhiệt và cáp mạng lỗi, mạng ổn định");
        var p9 = PhieuSuCo(bc7, new(2026, 5, 3, 14, 0, 0), new(2026, 5, 4, 8, 30, 0), new(2026, 5, 4, 9, 15, 0), TrangThaiPhieuXuLyEnum.HoanThanh, "Thay bàn phím, cổng USB hoạt động bình thường");
        var p10 = PhieuSuCo(bc6, new(2026, 10, 2, 8, 30, 0), null, null, TrangThaiPhieuXuLyEnum.ChoPhanCong);
        var phieus = new[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 };

        // ------------------------------------------------ PhanCongXuLy
        // Phieu 6: KTV5 (da ngung) -> doi sang KTV4: ban ghi cu DaKetThuc + ban ghi moi DangHieuLuc
        PhanCongXuLy Pc(PhieuXuLy p, KyThuatVien k, DateTime ngay, DateTime? ketThuc, string nd, TrangThaiPhanCongEnum tt) => new()
        {
            PhieuXuLy = p, KyThuatVien = k, NgayPhanCong = ngay, NgayKetThucPhanCong = ketThuc, NoiDungPhanCong = nd, TrangThai = tt
        };

        var phanCongs = new[]
        {
            Pc(p1, ktv1, new(2026, 6, 30, 10, 0, 0), new(2026, 7, 5, 11, 30, 0), "Bảo trì điều hòa Daikin", TrangThaiPhanCongEnum.DaKetThuc),
            Pc(p2, ktv1, new(2026, 10, 1, 10, 0, 0), null, "Bảo trì điều hòa phòng máy chủ", TrangThaiPhanCongEnum.DangHieuLuc),
            Pc(p3, ktv2, new(2026, 10, 2, 10, 30, 0), null, "Backup cấu hình và cập nhật firmware router", TrangThaiPhanCongEnum.DangHieuLuc),
            Pc(p4, ktv1, new(2026, 7, 10, 9, 30, 0), null, "Bảo trì máy chiếu phòng họp", TrangThaiPhanCongEnum.DangHieuLuc),
            Pc(p5, ktv3, new(2026, 4, 5, 9, 30, 0), new(2026, 4, 10, 10, 30, 0), "Bảo trì PC văn phòng", TrangThaiPhanCongEnum.DaKetThuc),
            Pc(p6, ktv5, new(2026, 9, 20, 9, 30, 0), new(2026, 9, 22, 8, 0, 0), "Kiểm tra máy in kẹt giấy", TrangThaiPhanCongEnum.DaKetThuc),
            Pc(p6, ktv4, new(2026, 9, 22, 8, 0, 0), null, "Tiếp nhận xử lý máy in thay KTV 5 (nghỉ)", TrangThaiPhanCongEnum.DangHieuLuc),
            Pc(p7, ktv3, new(2026, 8, 12, 15, 30, 0), new(2026, 8, 13, 11, 0, 0), "Xử lý máy tính chậm, treo", TrangThaiPhanCongEnum.DaKetThuc),
            Pc(p8, ktv2, new(2026, 9, 15, 10, 50, 0), null, "Xử lý sự cố mạng khẩn cấp", TrangThaiPhanCongEnum.DangHieuLuc),
            Pc(p9, ktv3, new(2026, 5, 3, 14, 30, 0), new(2026, 5, 4, 9, 15, 0), "Kiểm tra thiết bị ngoại vi", TrangThaiPhanCongEnum.DaKetThuc),
        };

        // ------------------------------------------------ VatTu (kho)
        var vtGas = new VatTu { TenVatTu = "Gas R32 (bình)", DonViTinh = "bình", DonGiaMacDinh = 350000m, SoLuongTon = 8 };
        var vtTu = new VatTu { TenVatTu = "Tụ khởi động 35uF", DonViTinh = "cái", DonGiaMacDinh = 180000m, SoLuongTon = 5 };
        var vtKeo = new VatTu { TenVatTu = "Keo tản nhiệt", DonViTinh = "tuýp", DonGiaMacDinh = 45000m, SoLuongTon = 20 };
        var vtConLan = new VatTu { TenVatTu = "Con lăn kéo giấy Canon", DonViTinh = "cái", DonGiaMacDinh = 120000m, SoLuongTon = 6 };
        var vtSsd = new VatTu { TenVatTu = "SSD 256GB", DonViTinh = "cái", DonGiaMacDinh = 850000m, SoLuongTon = 4 };
        var vtQuat = new VatTu { TenVatTu = "Quạt tản nhiệt 40mm", DonViTinh = "cái", DonGiaMacDinh = 95000m, SoLuongTon = 10 };
        var vtCap = new VatTu { TenVatTu = "Cáp mạng Cat6 (m)", DonViTinh = "mét", DonGiaMacDinh = 25000m, SoLuongTon = 100 };
        var vtPhim = new VatTu { TenVatTu = "Bàn phím Logitech K120", DonViTinh = "cái", DonGiaMacDinh = 220000m, SoLuongTon = 7 };
        var vatTus = new[] { vtGas, vtTu, vtKeo, vtConLan, vtSsd, vtQuat, vtCap, vtPhim };

        // ------------------------------------------------ ChiTietXuLy (ThanhTien = SoLuong * DonGia)
        ChiTietXuLy Ct(PhieuXuLy p, DateTime ngay, string nd, VatTu? vt = null, int? soLuong = null, string? ghiChu = null)
        {
            decimal? donGia = vt?.DonGiaMacDinh;
            return new ChiTietXuLy
            {
                PhieuXuLy = p, NgayCapNhat = ngay, NoiDungThucHien = nd,
                VatTu = vt, VatTuLinhKien = vt?.TenVatTu,
                SoLuong = vt == null ? null : soLuong,
                DonGia = vt == null ? null : donGia,
                ThanhTien = vt == null ? null : soLuong * donGia,
                GhiChu = ghiChu
            };
        }

        var chiTiets = new[]
        {
            Ct(p1, new(2026, 7, 5, 9, 30, 0), "Vệ sinh dàn nóng và dàn lạnh"),
            Ct(p1, new(2026, 7, 5, 10, 45, 0), "Nạp bổ sung gas", vtGas, 1),
            Ct(p2, new(2026, 10, 2, 10, 0, 0), "Tháo vệ sinh, kiểm tra block", ghiChu: "Block hoạt động yếu"),
            Ct(p2, new(2026, 10, 2, 14, 0, 0), "Thay tụ khởi động", vtTu, 1),
            Ct(p4, new(2026, 7, 18, 15, 0, 0), "Vệ sinh lọc bụi và kiểm tra bóng đèn"),
            Ct(p5, new(2026, 4, 10, 10, 0, 0), "Vệ sinh case, tra keo tản nhiệt", vtKeo, 1),
            Ct(p6, new(2026, 9, 21, 9, 30, 0), "Tháo cụm kéo giấy để kiểm tra", ghiChu: "Con lăn mòn"),
            Ct(p6, new(2026, 9, 23, 10, 0, 0), "Thay con lăn kéo giấy", vtConLan, 2, "Đang chờ chạy thử"),
            Ct(p7, new(2026, 8, 13, 9, 0, 0), "Cài lại Windows và driver"),
            Ct(p7, new(2026, 8, 13, 10, 30, 0), "Nâng cấp ổ cứng SSD", vtSsd, 1),
            Ct(p8, new(2026, 9, 15, 13, 0, 0), "Thay quạt tản nhiệt switch", vtQuat, 2),
            Ct(p8, new(2026, 9, 16, 8, 0, 0), "Thay cáp mạng lỗi", vtCap, 3),
            Ct(p9, new(2026, 5, 4, 9, 0, 0), "Thay bàn phím mới", vtPhim, 1),
        };

        // ------------------------------------------------ NhatKyDangNhap
        var nhatKys = new[]
        {
            new NhatKyDangNhap { TaiKhoan = admin, TenDangNhapNhap = "admin", ThoiGian = new(2026, 10, 2, 7, 55, 0), KetQua = KetQuaDangNhapEnum.ThanhCong, DiaChiIP = "192.168.1.10" },
            new NhatKyDangNhap { TaiKhoan = tkHung, TenDangNhapNhap = "ktv_hung", ThoiGian = new(2026, 10, 2, 8, 20, 0), KetQua = KetQuaDangNhapEnum.ThanhCong, DiaChiIP = "192.168.1.21" },
            new NhatKyDangNhap { TaiKhoan = uMai, TenDangNhapNhap = "user_mai", ThoiGian = new(2026, 10, 1, 9, 0, 0), KetQua = KetQuaDangNhapEnum.SaiMatKhau, DiaChiIP = "192.168.1.35" },
            new NhatKyDangNhap { TaiKhoan = uMai, TenDangNhapNhap = "user_mai", ThoiGian = new(2026, 10, 1, 9, 1, 0), KetQua = KetQuaDangNhapEnum.ThanhCong, DiaChiIP = "192.168.1.35" },
            new NhatKyDangNhap { TaiKhoan = uCu, TenDangNhapNhap = "user_cu", ThoiGian = new(2026, 9, 30, 17, 0, 0), KetQua = KetQuaDangNhapEnum.TaiKhoanBiKhoa, DiaChiIP = "192.168.1.50" },
            new NhatKyDangNhap { TaiKhoan = null, TenDangNhapNhap = "hacker01", ThoiGian = new(2026, 10, 2, 2, 13, 0), KetQua = KetQuaDangNhapEnum.KhongTonTaiTaiKhoan, DiaChiIP = "203.0.113.45" },
        };

        // ------------------------------------------------ ThongBao
        var thongBaos = new[]
        {
            new ThongBao { Kenh = KenhThongBaoEnum.HeThong, TaiKhoanNhan = admin, ThietBi = tb11, NoiDung = "Thiết bị 'UPS APC Smart-UPS 1500VA' đã quá hạn bảo trì (dự kiến 02/09/2026).", NgayTao = new(2026, 10, 3, 6, 0, 0), TrangThai = TrangThaiGuiThongBaoEnum.ChoGui },
            new ThongBao { Kenh = KenhThongBaoEnum.Email, TaiKhoanNhan = tkTuan, ThietBi = tb9, NoiDung = "Sự cố khẩn cấp: mạng chập chờn khu vực phòng máy chủ - phiếu xử lý đang chờ nghiệm thu.", NgayTao = new(2026, 9, 15, 10, 25, 0), NgayGui = new(2026, 9, 15, 10, 26, 0), TrangThai = TrangThaiGuiThongBaoEnum.DaGui, DaDoc = true },
            new ThongBao { Kenh = KenhThongBaoEnum.Email, TaiKhoanNhan = tkBao, ThietBi = tb4, NoiDung = "Bạn được phân công xử lý máy in Canon LBP6030 (kẹt giấy).", NgayTao = new(2026, 9, 22, 8, 0, 0), TrangThai = TrangThaiGuiThongBaoEnum.Loi },
        };

        // ------------------------------------------------ Luu (EF tu sap xep thu tu insert theo khoa ngoai)
        db.KhuVucs.AddRange(khuVucs);
        db.LoaiThietBis.AddRange(loais);
        db.TaiKhoans.AddRange(taiKhoans);
        db.KyThuatViens.AddRange(ktvs);
        db.ThietBis.AddRange(thietBis);
        db.KeHoachBaoTris.AddRange(keHoachs);
        db.BaoCaoSuCos.AddRange(baoCaos);
        db.PhieuXuLys.AddRange(phieus);
        db.PhanCongXuLys.AddRange(phanCongs);
        db.VatTus.AddRange(vatTus);
        db.ChiTietXuLys.AddRange(chiTiets);
        db.NhatKyDangNhaps.AddRange(nhatKys);
        db.ThongBaos.AddRange(thongBaos);
        db.SaveChanges();
    }
}