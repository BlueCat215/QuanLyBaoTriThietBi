# THIẾT KẾ CƠ SỞ DỮ LIỆU

Hệ thống Quản lý Bảo trì Thiết bị và Xử lý Sự cố — Đề tài 16, Nhóm UNETI16

**Phiên bản: 1.0 — 03/10/2026.** Bộ tài liệu gồm 3 file đồng bộ: [schema.dbml](./schema.dbml) (import vào dbdiagram.io để ra ERD), [db.sql](./db.sql) (script SQL Server đầy đủ) và file này.

## 1. Danh sách bảng theo Module

| Module | Người phụ trách | Bảng bắt buộc                                            |
| ------ | --------------- | -------------------------------------------------------- |
| 1      | SV1             | `TaiKhoan`, `LoaiThietBi`, `KhuVuc`                      |
| 2      | SV2             | `ThietBi`                                                |
| 3      | SV3             | `KeHoachBaoTri`                                          |
| 4      | SV4             | `BaoCaoSuCo`, `KyThuatVien`, `PhieuXuLy`, `PhanCongXuLy` |
| 5      | SV5             | `ChiTietXuLy`                                            |

| Bảng nâng cao (tùy chọn) | Phục vụ chức năng                                | Nên giao cho                                  |
| ------------------------ | ------------------------------------------------ | --------------------------------------------- |
| `NhatKyDangNhap`         | Bảo mật — audit đăng nhập, chống brute-force     | SV1 (cùng module Đăng nhập)                   |
| `TepDinhKem`             | Upload ảnh thiết bị/sự cố                        | SV2 (ThietBi) + SV4 (BaoCaoSuCo) cùng dùng    |
| `ThongBao`               | Email đến hạn bảo trì + thông báo trong hệ thống | SV5 (Dashboard/Thống kê hay đứng ra tổng hợp) |
| `VatTu`                  | Quản lý kho vật tư chi tiết                      | SV5 (ChiTietXuLy)                             |

## 2. Giải thích các quyết định thiết kế quan trọng

### 2.1. Các trường KHÔNG được lưu cột riêng (phải tính động bằng LINQ)

Đây là điểm nghiệp vụ đặc trưng của đề (mục 25), nên khi dựng Entity **tuyệt đối không** thêm các cột sau:

- `ThietBi.NgayBaoTriTiepTheo` — tính = `(NgayBaoTriGanNhat ?? NgayDuaVaoSuDung) + ChuKyBaoTriThang` (tháng).
- `ThietBi.TrangThaiHanBaoTri` (Chưa đến hạn/Sắp đến hạn/Quá hạn) — suy ra từ so sánh `NgayBaoTriTiepTheo` với ngày hiện tại. Ngưỡng "Sắp đến hạn" nhóm tự quy định (ví dụ 7 ngày) và dùng hằng số thống nhất 1 chỗ trong code.
- `KeHoachBaoTri.SoNgayQuaHan` — đề bài cấm rõ (mục 7.3), tính từ hiệu 2 ngày khi hiển thị.
- `ChiTietXuLy.ThanhTien` — có lưu cột (để không phải tính lại mỗi lần đọc chi tiết cũ), nhưng **không cho người dùng nhập**, Controller tự gán `= SoLuong * DonGia` trước khi `SaveChanges`.
- `PhieuXuLy.TongChiPhi` — **không** có cột này ở `PhieuXuLy`. Tổng chi phí = `SUM(ChiTietXuLy.ThanhTien)` theo `MaPhieuXuLy`, tính bằng LINQ mỗi khi cần hiển thị (trang chi tiết phiếu, Dashboard, thống kê).

### 2.2. Quan hệ 2 FK tùy loại ở `PhieuXuLy`

`PhieuXuLy` có cả `MaBaoCao` (nullable) và `MaKeHoach` (nullable) vì 1 phiếu xử lý có thể phát sinh từ báo cáo sự cố **hoặc** từ kế hoạch bảo trì định kỳ (`LoaiXuLy` phân biệt 2 nguồn). Ràng buộc "đúng 1 trong 2 phải có giá trị, cái còn lại null" không biểu diễn được bằng Data Annotation, nhưng DB ép được bằng CHECK cấp bảng `CK_Phieu_Nguon` (có trong `db.sql`, trong EF Core khai báo bằng `HasCheckConstraint`). Controller/Service vẫn kiểm tra trước khi `SaveChanges` để báo lỗi thân thiện, đúng yêu cầu mục 13 của đề.

### 2.3. Bảo toàn lịch sử khi đổi kỹ thuật viên

`PhanCongXuLy` có `TrangThai` (`DangHieuLuc`/`DaKetThuc`). Khi đổi KTV cho 1 phiếu: **không** `UPDATE` đè `MaKyThuatVien` của bản ghi cũ — set bản ghi cũ `TrangThai = DaKetThuc` + `NgayKetThucPhanCong = now`, rồi `INSERT` bản ghi `PhanCongXuLy` mới. Nhờ vậy truy vấn "KTV đang phụ trách hiện tại" = `Where(p => p.MaPhieuXuLy == x && p.TrangThai == DangHieuLuc)` và vẫn giữ nguyên lịch sử các lần đổi KTV trước đó (đúng mục 8.7).

### 2.4. `KyThuatVien` — quan hệ 1-0..1 với `TaiKhoan`

`KyThuatVien.MaTaiKhoan` là FK **nullable + unique**, không phải PK trùng với `TaiKhoan`. Tách riêng vì `KyThuatVien` còn có thuộc tính nghiệp vụ riêng (`ChuyenMon`...) mà `TaiKhoan` (dùng chung cho cả 3 vai trò) không cần. Unique đảm bảo 1 tài khoản chỉ gắn với đúng 1 hồ sơ KTV. Vì cột nullable, SQL Server chỉ cho đúng 1 dòng NULL với `UNIQUE` thường nên dùng **filtered unique index** `WHERE MaTaiKhoan IS NOT NULL` (đã có trong `db.sql`; EF Core `HasIndex().IsUnique()` tự thêm filter này).

### 2.5. Enum dùng cho các cột trạng thái

Toàn bộ `TrangThai`, `VaiTro`, `TinhTrang`, `MucDoUuTien`, `MucDoSuCo`, `LoaiXuLy` thiết kế dạng `enum` C#, **lưu dưới DB dạng chuỗi** (`nvarchar` + CHECK, cấu hình EF Core bằng `.HasConversion<string>()`) để khớp `db.sql` và dễ đọc khi debug; không dùng chuỗi tự do — để tránh nhập sai giá trị trạng thái và dễ kiểm tra chuyển trạng thái hợp lệ (workflow) tại Controller.

### 2.6. Xóa mềm thay vì xóa vật lý

Theo mục 6.2 của đề, `ThietBi` không nên xóa vật lý khi đã có lịch sử — thiết kế dùng sẵn cột `TrangThai = NgungSuDung` cho mục đích này, không cần cột `IsDeleted` riêng. Tương tự `LoaiThietBi`, `KhuVuc`, `KyThuatVien`, `TaiKhoan` đều có `TrangThai bit` để "khóa/ngừng" thay vì xóa, giữ toàn vẹn các bảng tham chiếu.

## 3. Bảng tổng hợp ràng buộc bắt buộc (map vào Data Annotation + kiểm tra ở Controller)

| Bảng          | Ràng buộc                                                         | Nơi kiểm tra                                                                                                                                          |
| ------------- | ----------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| TaiKhoan      | `TenDangNhap` unique, bắt buộc                                    | `[Required]` + kiểm tra LINQ trước khi Insert                                                                                                         |
| TaiKhoan      | `Email` đúng định dạng                                            | `[EmailAddress]`                                                                                                                                      |
| LoaiThietBi   | `TenLoaiThietBi` unique                                           | `[Required]` + LINQ                                                                                                                                   |
| LoaiThietBi   | `ChuKyBaoTriMacDinhThang > 0`                                     | `[Range(1, int.MaxValue)]`                                                                                                                            |
| ThietBi       | `SoSerial` unique                                                 | `[Required]` + LINQ                                                                                                                                   |
| ThietBi       | `NgayDuaVaoSuDung <= hôm nay`                                     | Validation tùy chỉnh (`IValidatableObject` hoặc kiểm tra ở Controller)                                                                                |
| ThietBi       | `ChuKyBaoTriThang > 0`                                            | `[Range]`                                                                                                                                             |
| KeHoachBaoTri | Không trùng kế hoạch định kỳ đang hoạt động cùng thiết bị/cùng kỳ | LINQ tại Controller/Service                                                                                                                           |
| BaoCaoSuCo    | Thiết bị chưa `NgungSuDung` mới được tạo báo cáo                  | Kiểm tra ở Controller                                                                                                                                 |
| KyThuatVien   | `TrangThai = 0` không được nhận `PhanCongXuLy` mới                | Kiểm tra ở Controller trước khi Insert `PhanCongXuLy`                                                                                                 |
| PhanCongXuLy  | Không có 2 bản ghi `DangHieuLuc` cho cùng `MaPhieuXuLy`           | LINQ tại Controller/Service + filtered unique index `UX_PhanCong_DangHieuLuc` (đổi KTV: set bản ghi cũ `DaKetThuc` trước, rồi mới insert bản ghi mới) |
| ChiTietXuLy   | `SoLuong > 0` (nếu có), `DonGia >= 0`                             | `[Range]`                                                                                                                                             |
| PhieuXuLy     | Đúng 1 trong 2 FK `MaBaoCao`/`MaKeHoach` theo `LoaiXuLy`          | CHECK `CK_Phieu_Nguon` + kiểm tra ở Service                                                                                                           |
| PhieuXuLy     | Chỉ chuyển `ChoNghiemThu` khi đã có `KetQuaXuLy`                  | Kiểm tra ở Controller                                                                                                                                 |
| PhieuXuLy     | Chỉ KTV đang `DangHieuLuc` trên phiếu mới được cập nhật           | Kiểm tra quyền ở Controller (so Session `MaTaiKhoan` ↔ `KyThuatVien` ↔ `PhanCongXuLy`)                                                                |

## 4. Thứ tự tạo Migration đề xuất

Để tránh lỗi FK khi `dotnet ef database update`, tạo Entity/DbSet theo đúng thứ tự phụ thuộc:

1. `LoaiThietBi`, `KhuVuc` (không phụ thuộc bảng nào khác)
2. `TaiKhoan` (phụ thuộc KhuVuc qua `MaKhuVuc`), `ThietBi` (phụ thuộc LoaiThietBi, KhuVuc)
3. `KyThuatVien` (phụ thuộc TaiKhoan)
4. `KeHoachBaoTri`, `BaoCaoSuCo` (phụ thuộc ThietBi, TaiKhoan)
5. `PhieuXuLy` (phụ thuộc BaoCaoSuCo, KeHoachBaoTri)
6. `PhanCongXuLy` (phụ thuộc PhieuXuLy, KyThuatVien)
7. `ChiTietXuLy` (phụ thuộc PhieuXuLy)

Cả nhóm nên thống nhất đủ 10 Entity trước (đúng quy trình mục 21: "Thiết kế CSDL → Thống nhất Entity → Tạo Project → DbContext/Migration"), rồi SV1 mới tạo **1 Migration đầu tiên duy nhất** chứa đủ 10 bảng, tránh 5 người tự ý `Add-Migration` rời rạc gây xung đột.

## 5. Các điểm đã rà soát và bổ sung

So với bản đầu, rà soát lại theo đúng yêu cầu đề bài thì có 3 chỗ đáng bổ sung — đã cập nhật trong `schema.dbml`:

### 5.1. Thiếu cơ chế giới hạn phạm vi xem của "Người sử dụng" (quan trọng nhất)

Mục 1 và 17.1 của đề nói Người sử dụng chỉ được **"xem thiết bị thuộc khu vực/phạm vi được phép"**, mục 13 nhắc lại **"Người sử dụng chỉ xem dữ liệu được phép"**. Bản thiết kế đầu chưa có chỗ nào lưu "phạm vi được phép" này — nếu không có, Controller không có dữ liệu để lọc theo quyền.

**Đã bổ sung:** cột `TaiKhoan.MaKhuVuc` (FK nullable tới `KhuVuc`). Với tài khoản `VaiTro = NguoiSuDung`, cột này xác định khu vực họ được xem thiết bị/gửi báo cáo sự cố; để `null` với Admin/Kỹ thuật viên vì 2 vai trò này không bị giới hạn theo khu vực. Đây là cách đơn giản nhất đáp ứng đúng yêu cầu — nếu thực tế nhóm cần 1 người dùng xem được **nhiều** khu vực, có thể nâng cấp thành bảng trung gian `TaiKhoan_KhuVuc` (n-n), nhưng với quy mô bài tập lớn thì 1-n là đủ và đơn giản hơn để bảo vệ.

### 5.2. Thiếu Index cho các cột lọc/sắp xếp nhiều

Mục 15 yêu cầu `ThietBi` **bắt buộc** Tìm kiếm + Lọc + Sắp xếp + Phân trang kết hợp, và `KeHoachBaoTri`/`BaoCaoSuCo`/`PhieuXuLy` cũng cần lọc theo trạng thái. Không có Index, các cột `MaLoaiThietBi`, `MaKhuVuc`, `TinhTrang`, `TrangThai`... sẽ bị quét toàn bảng (table scan) mỗi lần lọc — chạy đúng nhưng chậm dần khi dữ liệu mẫu tăng lên.

**Đã bổ sung:** khối `indexes { ... }` cho các cột hay dùng để lọc/tìm ở `ThietBi`, `KeHoachBaoTri`, `BaoCaoSuCo`, `PhieuXuLy`, `PhanCongXuLy`, `ChiTietXuLy`. Khi sinh Migration, EF Core sẽ tự tạo các Index này nếu khai báo `HasIndex()` trong `OnModelCreating` tương ứng.

### 5.3. Rủi ro Cascade Delete phá lịch sử

Mục 25 yêu cầu **bảo toàn lịch sử** bảo trì/sự cố/phân công/xử lý để truy vết toàn bộ quá trình. Mặc định EF Core Code First sinh FK với `ON DELETE CASCADE` — nếu không chỉnh lại, một thao tác xóa `ThietBi` (dù nghiệp vụ đã quy định dùng xóa mềm) vẫn có thể vô tình xóa dây chuyền toàn bộ `KeHoachBaoTri`/`BaoCaoSuCo`/`PhieuXuLy`/`PhanCongXuLy`/`ChiTietXuLy` liên quan nếu ai đó lỡ gọi `Remove()` trực tiếp thay vì đổi `TrangThai`.

**Đã bổ sung:** ghi chú cuối `schema.dbml` nhắc cấu hình toàn bộ FK liên quan là `DeleteBehavior.Restrict` trong `OnModelCreating`, kèm ví dụ code mẫu.

### 5.4. Những thứ cân nhắc nhưng KHÔNG thêm vào 10 bảng bắt buộc (phần ảnh, QR, vật tư đã được xử lý ở mục 6)

- Cột `NgayTao`/`NgaySua` audit chung cho mọi bảng — đề không yêu cầu, các bảng nghiệp vụ đã có đủ cột ngày riêng (`NgayBaoCao`, `NgayTao` của PhieuXuLy, `NgayCapNhat` của ChiTietXuLy...) để phục vụ lịch sử.
- Ảnh thiết bị/sự cố, mã QR — thuộc mục 19 "Chức năng nâng cao - không bắt buộc", chỉ thêm nếu nhóm chủ động chọn làm nâng cao.
- Bảng Vật tư/Kho riêng cho `ChiTietXuLy.VatTuLinhKien` — mục 19 liệt "Quản lý kho vật tư chi tiết" là nâng cao, giữ dạng chuỗi tên vật tư là đủ cho yêu cầu bắt buộc.

## 6. Bảng/cột bổ sung cho phần Nâng cao và Bảo mật (mục 19)

Toàn bộ phần này **tùy chọn** — chỉ làm nếu nhóm quyết định ghi điểm nâng cao, không làm vẫn đủ 5 Module bắt buộc.

### 6.1. Bảo mật tài khoản

- `TaiKhoan.SoLanDangNhapSai` + `KhoaDenNgay`: tự động khóa tạm 15–30 phút sau N lần (ví dụ 5 lần) đăng nhập sai liên tiếp — khác với `TrangThai = 0` là khóa vĩnh viễn do Admin chủ động khóa. Reset `SoLanDangNhapSai` về 0 ngay khi đăng nhập đúng.
- `TaiKhoan.MatKhauSalt`: **chỉ cần nếu nhóm tự viết hàm hash** (ví dụ tự implement PBKDF2/SHA256 + salt thủ công). Nếu dùng `PasswordHasher<T>` có sẵn của ASP.NET Core (khuyến nghị — ít code, đã được kiểm chứng an toàn), salt nằm chung trong chuỗi `MatKhau`, cột này bỏ trống.
- Bảng `NhatKyDangNhap`: ghi lại **mọi lần thử đăng nhập** (đúng lẫn sai), kể cả khi gõ sai tên đăng nhập (lúc đó `MaTaiKhoan` = null). Phục vụ trực tiếp kiểm thử mục 22.1 ("Đăng nhập đúng/sai", "Tài khoản bị khóa") và là bằng chứng cụ thể khi bảo vệ phần Session/phân quyền của SV1.
- **Khuyến nghị không dùng ASP.NET Core Identity đầy đủ**: Identity sẽ thay `TaiKhoan` bằng cấu trúc `AspNetUsers` (khóa chính kiểu `string`/GUID) và sinh thêm `AspNetRoles`, `AspNetUserRoles`... — lệch hoàn toàn với cấu trúc `TaiKhoan` (khóa `int`, các cột `VaiTro`/`TrangThai` cụ thể) mà đề bài yêu cầu ở mục 5.1. Dùng `PasswordHasher<T>` đứng độc lập (không cần cả hệ Identity) là đủ để có "Mã hóa mật khẩu" mà vẫn giữ đúng Entity bắt buộc.

### 6.2. Upload ảnh thiết bị/sự cố

Bảng `TepDinhKem` dùng chung 1 bảng cho nhiều loại đối tượng (`LoaiDoiTuong` + `MaDoiTuong`) thay vì thêm cột ảnh riêng vào từng bảng `ThietBi`/`BaoCaoSuCo`/`ChiTietXuLy` — linh hoạt cho phép nhiều ảnh/1 đối tượng. Vì `MaDoiTuong` là khóa ngoại "đa hình" (trỏ tới bảng khác nhau tùy `LoaiDoiTuong`), SQL Server không tạo được FK constraint thật — bắt buộc kiểm tra đối tượng tồn tại ở tầng Service/Controller trước khi insert.

### 6.3. Mã QR cho thiết bị

**Không cần thêm cột lưu mã QR.** Mã QR chỉ là ảnh encode 1 chuỗi (thường là URL `https://.../ThietBi/Detail/{MaThietBi}`) — sinh ra ngay lúc hiển thị/in bằng thư viện (ví dụ `QRCoder` NuGet) từ `MaThietBi` hoặc `SoSerial` đã có sẵn, không có gì để lưu vào DB. Lưu thêm cột chỉ tạo dữ liệu dư thừa phải đồng bộ khi `SoSerial` đổi.

### 6.4. Gửi email đến hạn bảo trì + thông báo thời gian thực

Gộp chung vào 1 bảng `ThongBao` với cột `Kenh` (`Email` hoặc `HeThong`) thay vì tách 2 bảng riêng — vì về bản chất đều là "1 thông điệp gửi cho 1 tài khoản, có trạng thái gửi". Lợi ích quan trọng nhất: **ghi log trước khi gửi** giúp job quét hằng ngày (tìm thiết bị sắp/quá hạn rồi tạo `ThongBao`) kiểm tra được "thiết bị này, kỳ hạn này đã gửi chưa" để không gửi trùng email mỗi ngày cho cùng 1 thiết bị đang quá hạn.

### 6.5. Quản lý kho vật tư

Bảng `VatTu` là danh mục (tên, đơn vị tính, đơn giá mặc định, số lượng tồn). `ChiTietXuLy` thêm `MaVatTu` (FK **nullable**) trỏ tới đây, nhưng **vẫn giữ nguyên** cột `VatTuLinhKien` (chuỗi) bắt buộc theo đề — khi kỹ thuật viên chọn vật tư có sẵn trong kho, hệ thống copy `TenVatTu` vào `VatTuLinhKien` tại thời điểm đó (snapshot). Lý do giữ snapshot thay vì chỉ dùng `MaVatTu`: nếu sau này đổi tên hoặc xóa `VatTu` trong danh mục, lịch sử `ChiTietXuLy` cũ không bị thay đổi theo — đúng nguyên tắc "bảo toàn lịch sử" của đề bài. Trừ kho (`SoLuongTon -= SoLuong`) thực hiện ở Service layer khi lưu `ChiTietXuLy`, không dùng trigger SQL để sinh viên dễ debug/giải thích khi bảo vệ.
