# BÁO CÁO TỰ MÔ TẢ KỸ THUẬT & BỘ CÂU HỎI PHẢN BIỆN BẢO VỆ ĐỒ ÁN
## PHÂN HỆ: MODULE 1 - QUẢN LÝ HỆ THỐNG, TÀI KHOẢN & DANH MỤC DÙNG CHUNG

---

- **Họ và tên sinh viên:** Nguyễn Văn Hoàng
- **Mã sinh viên:** 23103100303
- **Lớp:** TI17A4HN
- **Nhóm:** UNETI01
- **Đề tài:** Hệ thống Quản lý và Bảo trì Thiết bị (MaintainPro)
- **Công nghệ áp dụng:** ASP.NET Core 10 MVC, Entity Framework Core 10, Microsoft SQL Server, ClosedXML, C# 13
- **Phân hệ đảm nhiệm:** Module 1 (Hệ thống, Xác thực & Phân quyền, Nhật ký đăng nhập, Loại thiết bị, Khu vực)

---

## MỤC I: BÁO CÁO TỰ MÔ TẢ & GIẢI THÍCH KỸ THUẬT

### 1. Tổng quan nghiệp vụ Module 1 đảm nhiệm trong toàn bộ dự án
Module 1 đóng vai trò là **"Tầng nền tảng" (Foundation Layer)** của toàn bộ hệ thống MaintainPro. Nếu không có Module 1, toàn bộ các quy trình nghiệp vụ của các Module sau sẽ không thể khởi chạy do thiếu danh mục tham chiếu và cơ chế định danh người dùng:
1. **Quản lý Định danh & Tài khoản người dùng (`TaiKhoan`):** Quản lý tập trung toàn bộ người dùng trong tổ chức, phân loại theo 3 vai trò: `Admin` (Quản trị viên hệ thống), `KyThuatVien` (Kỹ thuật viên bảo trì/sửa chữa), `NguoiSuDung` (Cán bộ phụ trách phòng ban/khu vực).
2. **Cơ chế Xác thực & Điều hướng thông minh (`Auth`):** Xử lý đăng nhập, kiểm tra mật khẩu băm, quản lý phiên làm việc Session, điều hướng người dùng tới trang nghiệp vụ tương ứng sau khi đăng nhập thành công (`Admin` -> Dashboard, `KyThuatVien` -> Phiếu việc của tôi, `NguoiSuDung` -> Danh sách thiết bị).
3. **Bảo mật tài khoản & Chống tấn công Brute-force:** Tự động đếm số lần đăng nhập sai, kích hoạt khóa tạm thời 15 phút khi nhập sai liên tiếp 5 lần để ngăn chặn kẻ xấu dò vét mật khẩu bằng công cụ tự động.
4. **Nhật ký truy cập & Kiểm toán an ninh (`NhatKyDangNhap` - Audit Trail):** Ghi nhận 100% các lượt đăng nhập (thành công, sai mật khẩu, tài khoản bị khóa, tài khoản không tồn tại) kèm địa chỉ IP và dấu thời gian chính xác. Cung cấp giao diện tra cứu, bộ lọc nâng cao và chức năng xuất báo cáo Excel kiểm toán.
5. **Quản lý Danh mục dùng chung (Master Data / Lookup Tables):**
   - **Loại thiết bị (`LoaiThietBi`):** Thiết lập phân loại thiết bị, quy định chu kỳ bảo trì mặc định (tính theo tháng) làm cơ sở cho Module 2 tính hạn bảo trì và Module 3 lập kế hoạch định kỳ.
   - **Khu vực lắp đặt (`KhuVuc`):** Quản lý các phân xưởng, phòng máy, tòa nhà; làm cơ sở gán vị trí cho thiết bị (Module 2) và phân vùng phạm vi quản lý sự cố cho người sử dụng (Module 4).
6. **Bảo toàn dữ liệu & Chặn thao tác nguy hại (Safety Guard):** Ngăn chặn người dùng tự khóa hoặc tự xóa tài khoản của chính mình; đảm bảo hệ thống luôn tồn tại ít nhất một Quản trị viên (`Admin`) đang hoạt động; bảo vệ toàn vẹn dữ liệu bằng quy tắc Soft Delete / Ngừng hoạt động thay vì Hard Delete khi bản ghi đã phát sinh dữ liệu liên kết.

---

### 2. Mô tả kiến trúc 3 lớp (3-Tier Architecture) và luồng dữ liệu

Phân hệ được thiết kế tuân thủ nghiêm ngặt nguyên tắc **Tách biệt mối quan tâm (Separation of Concerns - SoC)** và **Đảo ngược phụ thuộc (Dependency Inversion Principle - DIP)** thông qua Dependency Injection container của ASP.NET Core:

```
[ Trình duyệt Client / View Razor ]
               │  ▲
               │  │  HTTP Request / Form Submit / AJAX / QueryString
               ▼  │
[ Controller Layer ]
  - AuthController, TaiKhoanController, LoaiThietBiController, KhuVucController, NhatKyDangNhapController
  - Đảm nhiệm: Kiểm tra phân quyền [Authorize], validation ModelState, nhận ViewModel, điều phối Service, trả View/File
               │  ▲
               │  │  DTO / ViewModel / ServiceResult<T>
               ▼  │
[ Service Layer (Nghiệp vụ ứng dụng) ]
  - AuthService, TaiKhoanService, LoaiThietBiService, KhuVucService, NhatKyDangNhapService
  - Đảm nhiệm: Băm & xác thực mật khẩu, kiểm tra chống brute-force, validate nghiệp vụ, kiểm tra ràng buộc xóa, ghi log
               │  ▲
               │  │  Entities, Linq Expressions, IQueryable
               ▼  │
[ Repository Layer (Truy xuất CSDL) ]
  - TaiKhoanRepository, LoaiThietBiRepository, KhuVucRepository, NhatKyDangNhapRepository
  - Đảm nhiệm: Tối ưu truy vấn EF Core, AsNoTracking(), phân trang Skip/Take, đếm số liệu thống kê
               │  ▲
               │  │  SQL Queries, Change Tracker
               ▼  │
[ Database Layer: Microsoft SQL Server (MaintainPro DB) ]
```

#### Chi tiết vai trò từng tầng:
- **Tầng Controllers:** Tiếp nhận request từ người dùng, gắn các bộ lọc bảo vệ phân quyền `[Authorize(Roles = AppRoles.Admin)]` hoặc `[AllowAnonymous]`. Controllers hoàn toàn "mỏng" (Thin Controllers), không chứa câu lệnh truy vấn CSDL hay logic nghiệp vụ tính toán.
- **Tầng Services:** Trung tâm xử lý logic nghiệp vụ. Sử dụng mẫu kết quả chuẩn hóa `ServiceResult` và `ServiceResult<T>` để đóng gói trạng thái thành công/thất bại và thông báo lỗi chi tiết, giúp tầng giao diện dễ dàng đẩy lỗi vào `ModelState` thông qua extension method `ModelState.AddServiceErrors(result)`.
- **Tầng Repositories:** Đóng gói toàn bộ các thao tác với Entity Framework Core. Tất cả các phương thức chỉ đọc đều kích hoạt `AsNoTracking()` hoặc sử dụng `QueryNoTracking()` kế thừa từ `Repository<T>` dùng chung, giúp giải phóng bộ nhớ Change Tracker và gia tăng tốc độ truy vấn từ 30% - 50%.

---

### 3. Thiết kế Cơ sở dữ liệu và chi tiết các Entity

#### 3.1. Entity `TaiKhoan`
- **Mã tài khoản (`MaTaiKhoan`):** Khóa chính tự tăng (Identity).
- **Định danh đăng nhập:** `TenDangNhap` (varchar 50, Unique Index `UQ_TaiKhoan_TenDangNhap` chặn tuyệt đối việc trùng lặp tên tài khoản ở cấp độ CSDL).
- **Bảo mật mật khẩu:** `MatKhau` (nvarchar 255) lưu trữ chuỗi mật khẩu đã được băm an toàn kèm muối (Salt) theo chuẩn PBKDF2 của ASP.NET Core Identity.
- **Thông tin cá nhân:** `HoTen` (nvarchar 100), `Email` (nvarchar 100).
- **Phân quyền & Phân vùng:** 
  - `VaiTro` (Enum `VaiTroEnum`: `Admin`, `KyThuatVien`, `NguoiSuDung`), kiểm soát bởi Check Constraint `CK_TaiKhoan_VaiTro`.
  - `MaKhuVuc` (FK trỏ tới `KhuVuc.MaKhuVuc`, nullable): Áp dụng riêng cho tài khoản người sử dụng để giới hạn phạm vi thiết bị được giám sát và gửi sự cố.
- **Trạng thái & Chống tấn công:**
  - `TrangThai` (bit): `true` = Hoạt động, `false` = Bị quản trị viên khóa vĩnh viễn.
  - `SoLanDangNhapSai` (int, default 0, Check Constraint `CK_TaiKhoan_SoLanSai` `>= 0`): Đếm số lần nhập sai mật khẩu liên tiếp.
  - `KhoaDenNgay` (datetime, nullable): Mốc thời gian khóa tạm thời khi nhập sai quá ngưỡng cho phép.
  - `NgayTaoTaiKhoan` (datetime, default `getdate()`): Ghi nhận thời điểm cấp phát tài khoản.
  - `LanDangNhapCuoi` (datetime, nullable): Dấu thời gian đăng nhập thành công gần nhất.

#### 3.2. Entity `NhatKyDangNhap` (Audit Log)
- **Mã nhật ký (`MaNhatKy`):** Khóa chính tự tăng (Identity).
- **Tài khoản liên kết (`MaTaiKhoan`):** Khóa ngoại trỏ về `TaiKhoan.MaTaiKhoan`, cho phép `NULL`. Điểm tinh tế trong thiết kế: khi kẻ tấn công nhập một username hoàn toàn không tồn tại trong hệ thống, hệ thống vẫn ghi lại bản ghi log với `MaTaiKhoan = NULL` để bảo đảm tính toàn vẹn của chuỗi kiểm toán.
- **Dữ liệu kiểm toán:** 
  - `TenDangNhapNhap` (nvarchar 50): Lưu nguyên văn chuỗi ký tự mà người dùng đã gõ trên form đăng nhập.
  - `ThoiGian` (datetime, default `getdate()`): Thời điểm phát sinh sự kiện đăng nhập. Được đánh chỉ mục `IX_NhatKy_ThoiGian` để tối ưu lọc theo khoảng ngày.
  - `KetQua` (Enum `KetQuaDangNhapEnum`: `ThanhCong`, `SaiMatKhau`, `TaiKhoanBiKhoa`, `KhongTonTaiTaiKhoan`).
  - `DiaChiIP` (varchar 45): Lưu trữ địa chỉ IPv4 hoặc IPv6 của client gửi request.
- **Nguyên tắc thiết kế bất biến (Immutable Audit Trail):** Bảng này chỉ hỗ trợ thao tác `INSERT` và `SELECT`. Repository và Service tuyệt đối không cung cấp hàm `Update` hoặc `Delete`, bảo đảm tính pháp lý và an toàn thông tin khi thanh tra hệ thống.

#### 3.3. Entity `LoaiThietBi`
- **Mã loại (`MaLoaiThietBi`):** Khóa chính tự tăng.
- **Tên loại (`TenLoaiThietBi`):** nvarchar(100), Unique Index `UQ_LoaiThietBi_Ten`.
- **Chu kỳ bảo trì mặc định (`ChuKyBaoTriMacDinhThang`):** int, Check Constraint `CK_LoaiThietBi_ChuKy` quy định `[ChuKyBaoTriMacDinhThang] > 0`. Trường này là giá trị tham chiếu để tự động điền chu kỳ khi thêm mới thiết bị ở Module 2.
- **Trạng thái:** `TrangThai` (bit, true = Đang sử dụng, false = Ngừng sử dụng).

#### 3.4. Entity `KhuVuc`
- **Mã khu vực (`MaKhuVuc`):** Khóa chính tự tăng.
- **Tên khu vực (`TenKhuVuc`):** nvarchar(100), Unique Index `UQ_KhuVuc_Ten`.
- **Thông tin vị trí:** `ViTri` (nvarchar 255), `NguoiPhuTrach` (nvarchar 100), `MoTa` (nvarchar 255).
- **Trạng thái:** `TrangThai` (bit, true = Đang hoạt động, false = Ngừng hoạt động).

#### 3.5. Bảng tổng hợp quan hệ dữ liệu trong Module 1:
| Thực thể nguồn | Quan hệ | Thực thể đích | Diễn giải nghiệp vụ |
|---|:---:|---|---|
| `KhuVuc` | $1 - N$ | `TaiKhoan` | Một khu vực có nhiều tài khoản người sử dụng phụ trách. |
| `KhuVuc` | $1 - N$ | `ThietBi` | Một khu vực chứa nhiều thiết bị máy móc đặt tại hiện trường. |
| `LoaiThietBi` | $1 - N$ | `ThietBi` | Một loại thiết bị áp dụng cho nhiều cá thể máy móc cụ thể. |
| `TaiKhoan` | $1 - N$ | `NhatKyDangNhap` | Một tài khoản có nhiều lượt đăng nhập qua thời gian. |
| `TaiKhoan` | $1 - 1$ | `KyThuatVien` | Tài khoản vai trò KTV liên kết $1 - 1$ với hồ sơ chuyên môn KTV. |
| `TaiKhoan` | $1 - N$ | `BaoCaoSuCo` | Một tài khoản người dùng có thể gửi nhiều báo cáo sự cố hư hỏng. |

---

### 4. Cơ chế Tìm kiếm, Lọc đa tiêu chí và Phân trang Server-side

#### 4.1. Kỹ thuật truy vấn IQueryable Deferred Execution
Toàn bộ logic lọc dữ liệu trong Module 1 được xây dựng theo mô hình **Đường ống biểu thức (LINQ Expression Pipeline)**. Câu truy vấn SQL chỉ được biên dịch và thực thi tại máy chủ CSDL khi gặp phương thức kết thúc (`ToListAsync()`, `CountAsync()`):

```csharp
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
        query = query.Where(x => x.KetQua == boLoc.KetQua.Value);

    if (boLoc.TuNgay.HasValue)
        query = query.Where(x => x.ThoiGian >= boLoc.TuNgay.Value.Date);

    if (boLoc.DenNgay.HasValue)
        query = query.Where(x => x.ThoiGian < boLoc.DenNgay.Value.Date.AddDays(1));

    return query;
}
```

#### 4.2. Phân trang Server-side với `PagedResult<T>`
Thay vì nạp toàn bộ hàng trăm ngàn bản ghi log về bộ nhớ Web Server rồi dùng `.ToPagedList()`, phân hệ áp dụng cơ chế phân trang thuần túy tại Database:
1. Đếm tổng số bản ghi thỏa mãn điều kiện lọc: `var total = await query.CountAsync();`
2. Phân trang bằng toán tử SQL Server `OFFSET ... FETCH NEXT`:
   ```csharp
   var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
   ```
3. Đóng gói vào cấu trúc `PagedResult<T>` gồm đầy đủ thông số: `Page`, `PageSize`, `TotalItems`, `TotalPages`, `HasPrevious`, `HasNext`.

#### 4.3. Cơ chế bảo toàn bộ lọc trên Query String khi chuyển trang
Trong View `Views/NhatKyDangNhap/Index.cshtml`, thẻ phân trang và nút xuất Excel sử dụng phương thức `boLoc.ToRouteValues(page)`:
```csharp
public Dictionary<string, string> ToRouteValues(int page)
{
    var dict = new Dictionary<string, string> { ["page"] = page.ToString() };
    if (!string.IsNullOrWhiteSpace(TuKhoa)) dict["tuKhoa"] = TuKhoa;
    if (KetQua.HasValue) dict["ketQua"] = KetQua.Value.ToString();
    if (TuNgay.HasValue) dict["tuNgay"] = TuNgay.Value.ToString("yyyy-MM-dd");
    if (DenNgay.HasValue) dict["denNgay"] = DenNgay.Value.ToString("yyyy-MM-dd");
    return dict;
}
```
Khi người dùng chuyển sang trang 2 hoặc bấm "Xuất Excel", tag helper `asp-all-route-data="@bl.ToRouteValues(p)"` sẽ tự động sinh đầy đủ Query String trên URL (ví dụ: `?tuKhoa=admin&ketQua=SaiMatKhau&tuNgay=2026-11-01&page=2`), giúp trạng thái tìm kiếm không bao giờ bị mất.

---

### 5. Các giải pháp kỹ thuật nâng cao triển khai trong Module 1

#### 5.1. Kiến trúc Xác thực tùy biến (Custom Session-based Authentication Handler)
Để tránh sự cồng kềnh của ASP.NET Core Identity mặc định (vốn tự động sinh ra hàng loạt bảng không cần thiết như `AspNetRoles`, `AspNetUserClaims`, `AspNetUserLogins`...), Module 1 triển khai một kiến trúc xác thực tinh gọn nhưng vô cùng mạnh mẽ:
- Khi đăng nhập thành công, `AuthController` lưu các thông tin cốt lõi vào Session: `MaTaiKhoan`, `TenDangNhap`, `HoTen`, `VaiTro`.
- Lớp `SessionAuthenticationHandler` kế thừa `AuthenticationHandler<AuthenticationSchemeOptions>` can thiệp vào pipeline của mỗi HTTP request, đọc Session và tự động kiến tạo đối tượng `ClaimsPrincipal` với các claims chuẩn:
  ```csharp
  var claims = new List<Claim>
  {
      new(ClaimTypes.NameIdentifier, maTaiKhoan.Value.ToString()),
      new("MaTaiKhoan", maTaiKhoan.Value.ToString()),
      new(ClaimTypes.Name, tenDangNhap),
      new("HoTen", hoTen),
      new(ClaimTypes.Role, vaiTro)
  };
  ```
- **Lợi ích kỹ thuật:** Giúp toàn bộ các Controller và Razor View trong toàn bộ dự án có thể sử dụng các tính năng chuẩn của .NET như:
  - Thuộc tính `[Authorize(Roles = AppRoles.Admin)]` hoặc `[Authorize(Roles = AppRoles.AdminOrKyThuatVien)]`.
  - Thuộc tính `@User.IsInRole("Admin")` trên giao diện sidebar.
  - Tự động chuyển hướng về `/Auth/DangNhap?returnUrl=...` khi chưa đăng nhập (Challenge) và chuyển hướng tới `/Auth/TuChoiTruyCap` (HTTP 403 Forbidden) khi không đủ quyền hạn.

#### 5.2. Cơ chế Bảo mật chống tấn công Brute-force & Băm mật khẩu PBKDF2
- **Thuật toán băm:** Sử dụng `PasswordHasher<TaiKhoan>` của Microsoft, triển khai thuật toán **PBKDF2** với hàm băm HMAC-SHA256, 100,000 vòng lặp (iterations) và muối (Salt) 128-bit được sinh ngẫu nhiên cho từng tài khoản. Chống lại hoàn toàn các cuộc tấn công tra cứu bảng băm sẵn (Rainbow Table).
- **Quy trình chống Brute-force:**
  1. Kiểm tra tài khoản có đang trong thời gian bị khóa tạm (`KhoaDenNgay > DateTime.Now`): nếu có, từ chối ngay lập tức và tính số phút còn lại mà **không thực hiện băm mật khẩu** (tiết kiệm CPU máy chủ và triệt tiêu khả năng timing attack).
  2. Khi kiểm tra mật khẩu thất bại (`VerifyHashedPassword == Failed`): tăng biến `SoLanDangNhapSai++`.
  3. Khi `SoLanDangNhapSai >= 5`: tự động tính toán `KhoaDenNgay = DateTime.Now.AddMinutes(15)` và reset bộ đếm `SoLanDangNhapSai = 0` (để sau khi hết 15 phút người dùng bắt đầu chu kỳ đếm mới).
  4. Khi đăng nhập thành công: lập tức xóa cờ khóa `KhoaDenNgay = null` và reset `SoLanDangNhapSai = 0`.

#### 5.3. Xuất báo cáo Kiểm toán Excel chuẩn hóa với ClosedXML
Module 1 tích hợp công cụ `ExcelExportHelper` generic dùng chung, cho phép xuất dữ liệu lịch sử đăng nhập ra file `.xlsx` theo định dạng doanh nghiệp:
- Bổ sung dòng tiêu đề báo cáo in hoa cỡ lớn, thông tin người xuất và thời điểm xuất báo cáo.
- Header cột nổi bật với mã màu nhận diện thương hiệu MaintainPro (`#1E40AF`), chữ trắng in đậm.
- Định dạng chuẩn: Ngày tháng hiển thị dạng `dd/MM/yyyy HH:mm:ss`, địa chỉ IP căn giữa, tên đăng nhập hiển thị dạng font đơn khoảng cách (monospace).
- Tự động căn chỉnh độ rộng cột (`AdjustToContents()`) và kẻ viền bảng (`Thin Border`).

#### 5.4. Cơ chế Xóa an toàn và Phòng vệ hệ thống (Self-locking & Last Admin Guard)
- **Chặn tự khóa / tự xóa chính mình:** Trong `TaiKhoanService`, phương thức kiểm tra mã tài khoản của người thực hiện (`maNguoiThucHien` lấy từ ClaimsPrincipal):
  ```csharp
  if (maNguoiThucHien == id)
      return ServiceResult.Fail("Bạn không thể tự khóa hoặc tự xóa tài khoản đang đăng nhập.");
  ```
- **Chặn xóa/khóa Quản trị viên cuối cùng:** Trước khi thay đổi vai trò, khóa hoặc xóa một tài khoản Admin, hệ thống kiểm tra:
  ```csharp
  if (await _repo.DemAdminHoatDongAsync(boQuaId: id) == 0)
      return ServiceResult.Fail("Hệ thống phải còn ít nhất một quản trị viên đang hoạt động.");
  ```
- **Bảo vệ toàn vẹn dữ liệu (Referential Integrity Guard):** Không bao giờ xóa cứng một `TaiKhoan`, `LoaiThietBi`, hay `KhuVuc` nếu đã có dữ liệu ràng buộc liên quan (thiết bị, sự cố, nhật ký...). Thay vào đó, giao diện tự động ẩn nút Xóa và hướng dẫn người dùng chuyển sang trạng thái "Ngừng sử dụng" / "Khóa tài khoản".

---

## MỤC II: BỘ CÂU HỎI VÀ CÂU TRẢ LỜI PHẢN BIỆN BẢO VỆ ĐỒ ÁN

### Câu 1: Tại sao em không sử dụng trọn bộ ASP.NET Core Identity có sẵn mà lại tự thiết kế bảng `TaiKhoan` và viết `SessionAuthenticationHandler`?
**Trả lời:**
- **Thứ nhất, về tính tinh gọn và kiểm soát kiến trúc:** ASP.NET Core Identity mặc định sinh ra tới 7-8 bảng liên quan (`AspNetUsers`, `AspNetRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`...) với các trường định danh dạng GUID và kiểu dữ liệu chuẩn quốc tế không hoàn toàn tương thích với yêu cầu nghiệp vụ quản lý tài sản doanh nghiệp Việt Nam. Việc tự thiết kế bảng `TaiKhoan` với khóa chính `int IDENTITY(1,1)` giúp cấu trúc CSDL gọn gàng, các quan hệ khóa ngoại với `KhuVuc`, `KyThuatVien`, `ThietBi` trực quan và đạt chuẩn thiết kế cơ sở dữ liệu.
- **Thứ hai, về khả năng tùy biến quy trình bảo mật:** Việc tự cài đặt cho phép em toàn quyền kiểm soát luồng xử lý đăng nhập: từ thuật toán đếm số lần sai liên tiếp, cơ chế khóa tạm 15 phút chống brute-force, cho đến việc ghi nhận toàn bộ lịch sử vào bảng kiểm toán `NhatKyDangNhap` (kể cả khi gõ sai username).
- **Thứ ba, về sự tương thích với chuẩn MVC:** Thông qua việc viết `SessionAuthenticationHandler`, em không làm mất đi các tính năng ưu việt của ASP.NET Core mà ngược lại, đã cầu nối thành công giữa dữ liệu phiên Session và cơ chế `ClaimsPrincipal`. Nhờ đó, toàn bộ các thuộc tính như `[Authorize(Roles = AppRoles.Admin)]`, `User.IsInRole()`, cơ chế bảo vệ CSRF và luồng xử lý Challenge/Forbidden (401/403) vẫn hoạt động hoàn toàn tự nhiên và chuẩn mực.

---

### Câu 2: Em hãy phân tích chi tiết cơ chế chống tấn công dò mật khẩu (Brute-Force Attack) trong Module của mình. Sự khác biệt giữa "Khóa tạm thời" và "Khóa quản trị" là gì?
**Trả lời:**
- **Cơ chế chống Brute-Force:**
  - Hệ thống sử dụng 2 trường trong bảng `TaiKhoan`: `SoLanDangNhapSai` (kiểu int) và `KhoaDenNgay` (kiểu datetime nullable).
  - Khi người dùng gửi yêu cầu đăng nhập, hàm `AuthService.DangNhapAsync` kiểm tra mốc `KhoaDenNgay`. Nếu `KhoaDenNgay > DateTime.Now`, tài khoản lập tức bị từ chối truy cập và trả về thông báo: *"Tài khoản đang bị tạm khóa. Vui lòng thử lại sau X phút"*. Điểm quan trọng là tại bước này, hệ thống **không chạy hàm kiểm tra băm mật khẩu**, giúp tiết kiệm tài nguyên tính toán của CPU và triệt tiêu khả năng kẻ tấn công đo đạc độ trễ phản hồi (Timing Attack).
  - Khi nhập sai mật khẩu, `SoLanDangNhapSai` tăng thêm 1. Khi chạm ngưỡng 5 lần, hệ thống tự động gán `KhoaDenNgay = DateTime.Now.AddMinutes(15)` và reset `SoLanDangNhapSai = 0`.
  - Khi nhập đúng mật khẩu, hệ thống xóa bỏ trạng thái khóa: gán `KhoaDenNgay = null` và `SoLanDangNhapSai = 0`.
- **Phân biệt Khóa tạm thời vs Khóa quản trị:**
  - **Khóa tạm thời (`KhoaDenNgay`):** Do hệ thống tự động kích hoạt khi có dấu hiệu bất thường (nhập sai 5 lần). Khóa có thời hạn (15 phút). Hết 15 phút, tài khoản tự động mở lại bình thường mà không cần sự can thiệp của con người.
  - **Khóa quản trị (`TrangThai = false`):** Do Quản trị viên (Admin) chủ động vô hiệu hóa tài khoản bằng thao tác nghiệp vụ. Khóa này có tính chất vĩnh viễn cho đến khi đích thân Quản trị viên bấm nút "Mở khóa". Khi tài khoản bị Admin khóa, kể cả người dùng gõ đúng mật khẩu thì hệ thống vẫn chặn lại ngay từ bước đầu tiên.

---

### Câu 3: Bảng `NhatKyDangNhap` phục vụ mục đích gì trong hệ thống? Tại sao trường `MaTaiKhoan` trong bảng này lại cho phép `NULL` và tại sao không bao giờ được phép xóa dữ liệu trong bảng này?
**Trả lời:**
- **Mục đích của bảng `NhatKyDangNhap`:** Đây là thành phần **Kiểm toán an ninh (Audit Trail)** của hệ thống. Nó ghi lại toàn bộ dấu vết lịch sử truy cập của mọi người dùng, giúp nhà quản lý phát hiện các nỗ lực xâm nhập trái phép, điều tra nguồn gốc sự cố an toàn thông tin (qua địa chỉ IP và dấu thời gian), đồng thời đánh giá tần suất sử dụng của các phòng ban.
- **Lý do trường `MaTaiKhoan` cho phép `NULL`:**
  - Khi một kẻ tấn công thực hiện hành vi dò quét (Reconnaissance) bằng cách nhập các tài khoản ngẫu nhiên như `root`, `administrator`, `test`... vốn không hề tồn tại trong cơ sở dữ liệu của công ty, nếu trường `MaTaiKhoan` bắt buộc `NOT NULL`, hệ thống sẽ không thể ghi lại nhật ký của đợt tấn công này do vi phạm ràng buộc khóa ngoại (Foreign Key Constraint).
  - Bằng cách cho phép `MaTaiKhoan NULL` và bổ sung trường `TenDangNhapNhap` lưu nguyên văn chuỗi kẻ xấu đã gõ, hệ thống bảo đảm lưu vết được 100% các cuộc tấn công thăm dò từ bên ngoài.
- **Lý do không bao giờ được phép xóa (Nguyên tắc bất biến - Immutability):**
  - Nhật ký kiểm toán là bằng chứng khách quan duy nhất để phục vụ công tác thanh tra và quy trách nhiệm khi xảy ra sự cố bảo mật. Nếu hệ thống cung cấp tính năng xóa log, một kẻ tấn công sau khi chiếm quyền Admin có thể dễ dàng xóa sạch dấu vết đăng nhập của mình để phi tang. Vì vậy, theo các chuẩn an toàn thông tin (như ISO 27001), bảng audit log chỉ hỗ trợ thao tác `INSERT` và `READ`, tuyệt đối không hỗ trợ `DELETE` hay `UPDATE`.

---

### Câu 4: Em làm thế nào để đảm bảo hệ thống không bao giờ rơi vào trạng thái "mồ côi quản trị" (Orphan System - tức là không còn bất kỳ Admin nào có thể đăng nhập)?
**Trả lời:**
Để phòng ngừa tuyệt đối kịch bản hệ thống không còn Quản trị viên, em đã thiết lập cơ chế **Bảo vệ Quản trị viên cuối cùng (Last Active Admin Guard)** tại tầng `TaiKhoanService`:
1. **Kiểm tra khi sửa tài khoản (`SuaAsync`):** Nếu một tài khoản đang là Admin hoạt động, nhưng quản trị viên thực hiện chỉnh sửa lại đổi vai trò sang `KyThuatVien`/`NguoiSuDung` hoặc gạt cờ `TrangThai = false` (khóa), Service sẽ gọi phương thức `_repo.DemAdminHoatDongAsync(boQuaId: tk.MaTaiKhoan)`. Nếu số lượng trả về bằng 0, hệ thống từ chối cập nhật và trả về thông báo lỗi: *"Hệ thống phải còn ít nhất một quản trị viên đang hoạt động."*
2. **Kiểm tra khi khóa tài khoản (`DoiTrangThaiAsync`):** Tương tự, nếu bấm khóa một tài khoản Admin, hệ thống kiểm tra nếu đây là Admin duy nhất còn lại đang hoạt động thì lập tức chặn lại.
3. **Kiểm tra khi xóa tài khoản (`XoaAsync`):** Áp dụng quy tắc tương tự trước khi thực hiện câu lệnh xóa.
4. **Quy tắc không tự thao tác chính mình:** Hệ thống luôn lấy `MaTaiKhoan` từ phiên đăng nhập hiện tại và chặn không cho Admin tự khóa, tự xóa, hoặc tự hạ quyền của chính mình. Sự kết hợp của các quy tắc này bảo đảm 100% hệ thống luôn có ít nhất một tài khoản Quản trị viên hoạt động để vận hành.

---

### Câu 5: Khi xóa một Loại thiết bị hoặc Khu vực, hệ thống xử lý như thế nào để đảm bảo tính toàn vẹn tham chiếu (Referential Integrity)? Tại sao em không dùng `CASCADE DELETE`?
**Trả lời:**
- **Xử lý toàn vẹn dữ liệu:**
  - Trong `LoaiThietBiService`, trước khi xóa một loại thiết bị, hệ thống thực hiện đếm số thiết bị đang trực thuộc: `await _repo.DemThietBiAsync(id)`. Nếu `soThietBi > 0`, Service lập tức chặn lại và báo lỗi: *"Không thể xóa: loại thiết bị đang có X thiết bị sử dụng. Hãy chuyển sang Ngừng sử dụng."*
  - Tương tự trong `KhuVucService`, hệ thống kiểm tra cả hai bảng con liên kết: số thiết bị tại khu vực (`DemThietBiAsync`) và số tài khoản người dùng gắn với khu vực (`DemTaiKhoanAsync`). Nếu có bất kỳ dữ liệu nào liên quan, thao tác xóa sẽ bị từ chối.
  - Trên giao diện Razor View, em sử dụng điều kiện kiểm tra dữ liệu: chỉ những bản ghi có số liên kết bằng 0 mới hiển thị nút biểu tượng "Thùng rác" (Xóa); các bản ghi đã có dữ liệu liên kết chỉ hiển thị nút đổi trạng thái hoạt động/ngừng hoạt động.
- **Tại sao không dùng `CASCADE DELETE`:**
  - `CASCADE DELETE` là cơ chế cực kỳ nguy hiểm trong hệ thống quản lý tài sản doanh nghiệp. Nếu bật cascade delete trên bảng `LoaiThietBi`, khi vô tình xóa một loại thiết bị (ví dụ: "Máy biến áp"), SQL Server sẽ tự động xóa sạch toàn bộ hàng trăm máy biến áp con, kéo theo xóa sạch toàn bộ lịch sử kế hoạch bảo trì, báo cáo sự cố và phiếu xử lý liên quan trong quá khứ.
  - Điều này gây mất mát dữ liệu nghiêm trọng và phá hủy hoàn toàn lịch sử kế toán, bảo trì của doanh nghiệp. Vì vậy, em áp dụng mô hình **Xóa an toàn có bảo vệ (Restrict Delete / Soft State)**: dữ liệu đã phát sinh giao dịch thì chỉ được phép ngừng hoạt động, không được xóa vật lý.

---

### Câu 6: Trong phân trang danh sách Nhật ký đăng nhập, tại sao em phải dùng `PagedResult<T>` và `IQueryable` thay vì lấy hết về RAM rồi dùng `.ToPagedList()`?
**Trả lời:**
- **Về mặt hiệu năng và tài nguyên bộ nhớ (Memory Consumption):**
  - Nhật ký đăng nhập là bảng có tốc độ tăng trưởng dữ liệu rất nhanh. Trong một nhà máy hoạt động 1 năm, bảng này có thể lên tới 500,000 đến 1,000,000 bản ghi.
  - Nếu dùng phương pháp nạp toàn bộ vào RAM (ví dụ: `_context.NhatKyDangNhaps.ToList().ToPagedList(page, pageSize)`), Web Server sẽ phải tải hàng trăm megabyte dữ liệu từ SQL Server qua mạng về RAM chỉ để lấy ra đúng 20 dòng hiển thị cho người dùng. Điều này sẽ khiến RAM của máy chủ web bị quá tải (Out of Memory) và làm nghẽn băng thông đường truyền giữa App Server và Database Server.
- **Về mặt tối ưu hóa câu lệnh SQL (Database Server Execution):**
  - Khi dùng `IQueryable` kết hợp `.Skip((page - 1) * pageSize).Take(pageSize)`, Entity Framework Core sẽ biên dịch LINQ thành câu lệnh SQL Server chuẩn sử dụng mệnh đề:
    `OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY;`
  - Nhờ đó, máy chủ SQL Server chỉ quét chỉ mục và trả về qua đường truyền đúng 20 bản ghi của trang hiện tại, thời gian phản hồi (Response Time) luôn duy trì ở mức một vài mili-giây bất kể bảng dữ liệu có hàng triệu dòng.

---

### Câu 7: Tại sao mật khẩu người dùng không được lưu dạng chuỗi thông thường (Plaintext) hoặc mã hóa MD5/SHA256 đơn thuần? Cơ chế `PasswordHasher<T>` mà em sử dụng hoạt động như thế nào?
**Trả lời:**
- **Nguy cơ của Plaintext và Hash thông thường (MD5/SHA256):**
  - Lưu Plaintext vi phạm nghiêm trọng luật an toàn thông tin; nếu file backup CSDL bị rò rỉ, toàn bộ mật khẩu người dùng sẽ bị lộ.
  - Sử dụng các hàm băm đơn thuần như MD5 hay SHA256 mà không có muối (Salt) thì cực kỳ dễ bị giải mã thông qua **Tấn công từ điển (Dictionary Attack)** và **Bảng băm tính sẵn (Rainbow Table)**. Thêm vào đó, các thuật toán như SHA256 được thiết kế để tính toán rất nhanh trên phần cứng hiện đại (GPU có thể thử hàng tỷ hash/giây), khiến kẻ xấu có thể brute-force mật khẩu ngoại tuyến rất nhanh chóng.
- **Cơ chế của `PasswordHasher<TaiKhoan>`:**
  - `PasswordHasher<T>` của ASP.NET Core sử dụng chuẩn **PBKDF2** (Password-Based Key Derivation Function 2) với thuật toán HMAC-SHA256.
  - **Sinh muối ngẫu nhiên (Cryptographic Salt):** Mỗi lần băm một mật khẩu, hệ thống sinh ra một chuỗi muối ngẫu nhiên 128-bit bằng bộ sinh số ngẫu nhiên an toàn (CSPRNG). Do đó, hai tài khoản có cùng mật khẩu là `"123456"` thì chuỗi băm lưu trong CSDL vẫn hoàn toàn khác biệt nhau, vô hiệu hóa hoàn toàn Rainbow Table.
  - **Kéo dãn khóa (Key Stretching):** Thuật toán thực hiện lặp đi lặp lại 100,000 vòng (Work Factor), làm chậm quá trình tính toán của kẻ tấn công xuống mức hàng triệu lần, khiến việc dùng GPU để dò mật khẩu trở nên bất khả thi về mặt thời gian và chi phí phần cứng.
  - Định dạng chuỗi lưu trữ bao gồm: Header phiên bản format, ID thuật toán PRF, Salt, và Subkey băm; khi người dùng đăng nhập, hàm `VerifyHashedPassword` sẽ tự trích xuất Salt từ chuỗi băm cũ để băm mật khẩu vừa nhập và so sánh kết quả theo kỹ thuật hằng số thời gian (Constant-time comparison) để chống Timing Attack.

---

### Câu 8: Cơ chế phân quyền RBAC (Role-Based Access Control) hoạt động như thế nào từ lúc người dùng đăng nhập đến khi truy cập một Action có `[Authorize(Roles = AppRoles.Admin)]`?
**Trả lời:**
Toàn bộ quy trình diễn ra theo 5 bước tuần tự và chặt chẽ:
1. **Đăng nhập & Thiết lập Session:** Người dùng nhập thông tin tại `AuthController.DangNhap`. Sau khi xác thực đúng tên đăng nhập và mật khẩu, Controller lưu giá trị `VaiTro` (ví dụ: `Admin`) vào Session của người dùng thông qua `HttpContext.Session.SetString(SessionKeys.VaiTro, "Admin")`.
2. **Can thiệp Middleware & Dựng ClaimsPrincipal:** Ở mỗi request tiếp theo gửi từ trình duyệt, trước khi vào tới Controller, `AuthenticationMiddleware` kích hoạt `SessionAuthenticationHandler.HandleAuthenticateAsync()`. Handler này đọc Session của request hiện tại; nếu tìm thấy thông tin đăng nhập, nó tạo một `Claim` kiểu `ClaimTypes.Role` với giá trị `"Admin"`, sau đó bọc vào `ClaimsIdentity` và gán vào thuộc tính `HttpContext.User`.
3. **Kiểm tra tại Authorization Filter:** Khi request muốn tiến vào một Action có đánh dấu `[Authorize(Roles = AppRoles.Admin)]` (ví dụ trong `TaiKhoanController`), bộ lọc phân quyền của ASP.NET Core sẽ đọc danh sách role được phép (`AppRoles.Admin`) và gọi hàm `HttpContext.User.IsInRole("Admin")`.
4. **Xử lý khi hợp lệ:** Nếu kết quả trả về `true`, request được phép tiếp tục đi vào thân hàm Action để thực thi và hiển thị trang cho người dùng.
5. **Xử lý khi bất hợp lệ:** 
   - Nếu người dùng chưa hề đăng nhập (`HttpContext.User.Identity.IsAuthenticated == false`): Handler kích hoạt phương thức `HandleChallengeAsync`, tự động chuyển hướng người dùng về trang đăng nhập `/Auth/DangNhap?returnUrl=...`.
   - Nếu người dùng đã đăng nhập nhưng là vai trò `NguoiSuDung` (không có quyền Admin): Handler kích hoạt `HandleForbiddenAsync`, thiết lập HTTP Status Code là **403 Forbidden** và chuyển hướng người dùng về trang `/Auth/TuChoiTruyCap`, ngăn chặn hoàn toàn việc leo thang đặc quyền.
