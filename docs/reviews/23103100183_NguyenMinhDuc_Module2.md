# BÁO CÁO TỰ MÔ TẢ & BỘ CÂU HỎI PHẢN BIỆN BẢO VỆ ĐỒ ÁN
## MODULE 2: QUẢN LÝ THIẾT BỊ (MAINTAINPRO)

**Học phần:** Lập trình .NET / Đồ án phát triển phần mềm  
**Nhóm sinh viên thực hiện:** UNETI01 - Lớp TI17A4HN  
**Sinh viên đảm nhiệm:** Nguyễn Minh Đức  

---

## PHẦN 1: BÁO CÁO TỰ MÔ TẢ & GIẢI THÍCH KỸ THUẬT

### 1. Giới thiệu tổng quan Module 2
Module 2 là phân hệ trung tâm của hệ thống **MaintainPro**, chịu trách nhiệm quản lý toàn bộ danh mục tài sản máy móc, thiết bị kỹ thuật trong nhà xưởng/phòng máy. Phân hệ cung cấp các nghiệp vụ:
- Quản lý danh mục thiết bị (Thêm, Sửa, Xem chi tiết, Thay đổi trạng thái, Xóa an toàn).
- Tìm kiếm toàn văn, lọc đa tiêu chí (theo loại thiết bị, khu vực, tình trạng vận hành, trạng thái hạn bảo trì, khoảng thời gian đưa vào sử dụng), sắp xếp linh hoạt và phân trang Server-side.
- Xuất danh sách thiết bị ra file Excel theo đúng điều kiện lọc hiện tại.
- Tính toán động chu kỳ bảo trì, cảnh báo thiết bị quá hạn hoặc sắp đến hạn bảo trì.
- Quản lý bộ sưu tập ảnh thực tế của thiết bị với cơ chế tải nhiều tệp và xem phóng to (Lightbox).
- Tạo mã QR Code động dẫn đến trang lý lịch thiết bị và giao diện in tem nhãn tài sản dán máy chuẩn kích thước 80x50mm.

---

### 2. Kiến trúc & Các thành phần triển khai

Phân hệ được tổ chức theo kiến trúc 3 lớp phân tách rõ ràng trách nhiệm (Separation of Concerns):

```
Controllers/ThietBi/ThietBiController.cs
  │ (Xử lý HTTP Request, ModelState, Authorization, View/File Result)
  ▼
Services/ThietBi/ThietBiService.cs
  │ (Xử lý nghiệp vụ: tính hạn bảo trì, kiểm tra toàn vẹn, điều phối upload ảnh, mapper ViewModel)
  ▼
Repositories/ThietBi/ThietBiRepository.cs & ThietBiQueryExtensions.cs
  │ (Truy vấn CSDL qua EF Core, IQueryable pipeline, Skip/Take phân trang)
  ▼
Models/ThietBi.cs (Entity) & ApplicationDbContext
```

#### A. Cấu trúc CSDL và Entity `ThietBi`
- **Mã thiết bị (`MaThietBi`):** Khóa chính tự tăng (Identity).
- **Thông tin nhận diện:** `TenThietBi` (nvarchar 100), `SoSerial` (varchar 50, Unique Index chặn trùng lặp).
- **Khóa ngoại liên kết:** 
  - `MaLoaiThietBi` -> Bảng `LoaiThietBi` (Module 1).
  - `MaKhuVuc` -> Bảng `KhuVuc` (Module 1).
- **Thông số bảo trì:** 
  - `ChuKyBaoTriThang`: Chu kỳ bảo trì định kỳ tính theo tháng (>= 1).
  - `NgayDuaVaoSuDung`: Ngày bắt đầu vận hành máy.
  - `NgayBaoTriGanNhat`: Ngày hoàn thành đợt bảo trì gần nhất (nullable, nếu chưa bảo trì mốc tính sẽ căn cứ từ ngày đưa vào sử dụng).
- **Trạng thái:** `TinhTrang` (Enum: `DangHoatDong`, `DangBaoTri`, `DangSuaChua`, `NgungSuDung`).
- **Thuộc tính tính toán động (`[NotMapped]`):**
  - `NgayBaoTriTiepTheo`: Xác định bằng công thức `(NgayBaoTriGanNhat ?? NgayDuaVaoSuDung) + ChuKyBaoTriThang`.
  - `TrangThaiHanBaoTri`: Phân loại thành `QuaHan`, `SapDenHan` (<= 7 ngày), `BinhThuong`, `NgungSuDung`.

#### B. Cơ chế Tìm kiếm, Lọc, Sắp xếp và Phân trang (SFSP)
- **Pipeline LINQ mở rộng (`ThietBiQueryExtensions`):**
  - `LocTheoTuKhoa`: Tìm kiếm không dấu / có dấu trên cả 3 trường `TenThietBi`, `SoSerial` và `TenKhuVuc`.
  - `LocTheoDanhMuc`: Lọc chính xác theo `MaLoaiThietBi` và `MaKhuVuc`.
  - `LocTheoTinhTrang`: Lọc theo enum tình trạng thiết bị.
  - `LocTheoHanBaoTri`: Chuyển đổi logic tính hạn thành biểu thức SQL tương thích EF Core để lọc trực tiếp trên máy chủ SQL, tránh nạp toàn bộ bảng vào RAM.
  - `LocTheoKhoangNgay`: Lọc `NgayDuaVaoSuDung` từ ngày đến ngày.
  - `SapXepThietBi`: Hỗ trợ sắp xếp theo Tên (A-Z, Z-A), Ngày đưa vào sử dụng (Mới nhất, Cũ nhất), Ngày bảo trì gần nhất, và Hạn bảo trì tiếp theo.
- **Phân trang an toàn (`PagedResult<T>`):**
  - Phân trang hoàn toàn ở cấp Database thông qua `.Skip((trang - 1) * pageSize).Take(pageSize)`.
  - Bảo toàn đầy đủ giá trị Query String khi chuyển trang bằng phương thức `bl.ToRouteValues(page)`.

#### C. Quản lý Ảnh thực tế của thiết bị (Đa hình)
- Tích hợp dịch vụ `ITepDinhKemService` theo mô hình quan hệ đa hình (Polymorphic Association):
  - `LoaiDoiTuong`: Lưu giá trị `ThietBi` (Enum `LoaiDoiTuongDinhKemEnum`).
  - `MaDoiTuong`: Lưu `MaThietBi`.
- **Cơ chế an toàn:**
  - Kiểm tra đuôi file (Whitelist: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`), chặn đứng các loại tệp thực thi (`.exe`, `.php`, `.asp`, `.bat`).
  - Giới hạn dung lượng mỗi ảnh tối đa 5MB.
  - Đổi tên tệp lưu trên ổ đĩa thành chuỗi định danh ngẫu nhiên (GUID) kết hợp ngày giờ để chống ghi đè và tấn công Path Traversal.
- **Trải nghiệm người dùng:**
  - Form Thêm/Sửa thiết bị cho phép chọn cùng lúc nhiều ảnh thực tế (`multipart/form-data`).
  - Trang chi tiết thiết bị tích hợp Gallery trực quan và Lightbox Modal phóng to ảnh kèm nút tải ảnh gốc.

#### D. Mã QR Động & In Tem Nhãn Tài Sản (Chuẩn nhiệt 80x50mm)
- **Cơ chế sinh mã QR:**
  - Sử dụng thư viện `QRCoder` đóng gói trong `QRCodeHelper`.
  - Không lưu tệp ảnh QR tĩnh vào đĩa hay CSDL; mã QR được sinh động tại runtime dưới dạng Base64 Data URI (`data:image/png;base64,...`) đưa trực tiếp vào thẻ `<img>`.
  - Nội dung nhúng trong mã QR là URL tuyệt đối dẫn tới trang chi tiết thiết bị (`/ThietBi/Details/{id}`). Người dùng tại hiện trường dùng camera điện thoại quét là mở được ngay lý lịch máy để xem thông số hoặc gửi báo cáo sự cố.
- **In tem nhãn dán máy (`InTemNhan.cshtml`):**
  - Thiết kế layout độc lập (`Layout = null`), định dạng chuẩn khổ giấy in tem nhiệt công nghiệp **80mm x 50mm**.
  - Hiển thị đầy đủ: Logo/Thương hiệu hệ thống, Mã thiết bị, Mã QR chất lượng cao, Tên máy, Số serial (màu đỏ nổi bật), Loại máy, Khu vực và Ngày in.
  - Sử dụng CSS `@media print` với `@page { size: 80mm 50mm; margin: 0; }` để khi bấm nút "In ngay" hoặc Ctrl+P, máy in nhiệt sẽ in ra đúng kích thước tem không bị lệch viền, tự động ẩn thanh công cụ điều hướng.

---

## PHẦN 2: BỘ CÂU HỎI VÀ TRẢ LỜI PHẢN BIỆN KHI BẢO VỆ

### Câu 1: Tại sao trường `NgayBaoTriTiepTheo` và `TrangThaiHanBaoTri` không được tạo thành cột trong CSDL mà lại dùng thuộc tính tính toán `[NotMapped]`?
**Trả lời:**
- **Về mặt thiết kế CSDL (Chuẩn hóa dữ liệu):** Hạn bảo trì tiếp theo phụ thuộc trực tiếp vào `ChuKyBaoTriThang` và ngày bảo trì gần nhất (`NgayBaoTriGanNhat ?? NgayDuaVaoSuDung`). Nếu lưu thành một cột tĩnh trong CSDL, dữ liệu sẽ bị dư thừa và có nguy cơ sai lệch dữ liệu (Data Inconsistency) khi người dùng cập nhật chu kỳ hoặc khi có một phiếu bảo trì mới được nghiệm thu.
- **Về mặt nghiệp vụ:** Trạng thái hạn bảo trì (`QuaHan`, `SapDenHan`, `BinhThuong`) biến đổi liên tục theo thời gian thực (so với ngày hôm nay `DateTime.Today`). Nếu lưu cột tĩnh, hệ thống sẽ phải chạy background job hàng đêm để quét và update lại trạng thái cho hàng ngàn bản ghi. Thay vào đó, em thiết kế `[NotMapped]` và viết biểu thức tính toán trong LINQ để SQL Server tự tính toán ngay tại thời điểm truy vấn, vừa đảm bảo tính chính xác 100% theo thời gian thực, vừa giảm tải thao tác ghi trên CSDL.

---

### Câu 2: Trong chức năng phân trang, làm thế nào để khi người dùng chuyển sang trang khác mà không làm mất các điều kiện tìm kiếm, lọc và sắp xếp đang chọn?
**Trả lời:**
- Khi người dùng gửi form lọc, request được gửi theo phương thức `GET` để toàn bộ tiêu chí (Từ khóa, Loại thiết bị, Khu vực, Tình trạng, Hạn bảo trì, v.v.) xuất hiện trên Query String của URL.
- Trong class `ThietBiBoLoc`, em xây dựng phương thức `ToRouteValues(int trangMoi)`. Phương thức này nhận vào số trang cần chuyển đến và trả về một `Dictionary<string, object>` chứa toàn bộ các giá trị lọc hiện tại kèm theo chỉ số trang mới.
- Tại giao diện Razor View, các link phân trang sử dụng thẻ tag helper:
  `asp-action="Index" asp-all-route-data="@Model.BoLoc.ToRouteValues(i)"`.
- Nhờ vậy, URL luôn duy trì đầy đủ các tham số lọc khi chuyển trang, người dùng có thể bookmark lại URL hoặc chia sẻ link kết quả tìm kiếm cho đồng nghiệp mà vẫn giữ nguyên kết quả.

---

### Câu 3: Tại sao lại tách riêng tầng Repository (`ThietBiRepository`) và tầng Service (`ThietBiService`) thay vì gọi trực tiếp `DbContext` ngay trong Controller?
**Trả lời:**
- **Tuân thủ nguyên lý Single Responsibility & Separation of Concerns:**
  - `ThietBiController` chỉ đóng vai trò nhận request từ client, kiểm tra tính hợp lệ cơ bản của dữ liệu đầu vào (`ModelState`), điều hướng và trả về View hoặc File kết quả.
  - `ThietBiService` đảm nhận toàn bộ nghiệp vụ (Business Logic): kiểm tra điều kiện ràng buộc xóa an toàn, kiểm tra xem số Serial có bị trùng không, gọi dịch vụ upload ảnh, tính toán ViewModel.
  - `ThietBiRepository` đóng vai trò là tầng trừu tượng hóa việc truy cập dữ liệu (Data Access Layer), gom nhóm các câu lệnh LINQ / Entity Framework Core vào một nơi duy nhất.
- **Khả năng bảo trì và kiểm thử:** Nếu sau này hệ thống thay đổi CSDL hoặc cần viết Unit Test, ta chỉ việc Mock interface `IThietBiRepository` và `IThietBiService` mà không cần phụ thuộc vào kết nối CSDL thực tế hay Web server.

---

### Câu 4: Cơ chế upload ảnh thiết bị được tổ chức như thế nào? Bảng `TepDinhKem` không có khóa ngoại Foreign Key trỏ trực tiếp đến `ThietBi`, vậy làm sao đảm bảo liên kết dữ liệu?
**Trả lời:**
- Em sử dụng mô hình **Quan hệ đa hình (Polymorphic Association)** thông qua bảng dùng chung `TepDinhKem`.
- Cấu trúc bảng `TepDinhKem` gồm cặp trường định danh:
  - `LoaiDoiTuong`: Lưu kiểu đối tượng (được chuẩn hóa bằng enum `LoaiDoiTuongDinhKemEnum`, ở module này là `ThietBi = 1`).
  - `MaDoiTuong`: Lưu ID của đối tượng đó (`MaThietBi`).
- **Ưu điểm kiến trúc:** Thiết kế này giúp hệ thống không cần tạo nhiều bảng ảnh riêng biệt (như `AnhThietBi`, `TepBaoTri`, `TepSuCo`), giúp tái sử dụng hoàn toàn logic upload, kiểm tra định dạng, kiểm tra dung lượng và xóa file vật lý cho tất cả các module khác trong dự án.
- **Đảm bảo liên kết dữ liệu:** Tầng Service chịu trách nhiệm truy vấn `TepDinhKem` theo đúng cặp `(LoaiDoiTuong == ThietBi && MaDoiTuong == maThietBi)`. Khi xóa một thiết bị hoặc xóa một ảnh, Service sẽ dọn dẹp cả bản ghi trong CSDL lẫn tệp vật lý trong thư mục `wwwroot/uploads` để tránh phát sinh dữ liệu rác.

---

### Câu 5: Bạn bảo mật tính năng Upload ảnh như thế nào để ngăn chặn mã độc (Webshell, Virus, Path Traversal)?
**Trả lời:**
Trong `TepDinhKemService`, em áp dụng 4 lớp kiểm tra an toàn nghiêm ngặt:
1. **Kiểm tra phần mở rộng (Extension Whitelist):** Chỉ chấp nhận các định dạng ảnh hợp lệ: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`. Tuyệt đối chặn các đuôi tệp thực thi như `.exe`, `.bat`, `.cmd`, `.ps1`, `.sh`, `.php`, `.asp`, `.aspx`.
2. **Kiểm tra MIME Content-Type:** Đối chiếu Content-Type từ request (ví dụ: `image/jpeg`, `image/png`) để đảm bảo tệp tải lên đúng là ảnh.
3. **Giới hạn kích thước tệp:** Giới hạn mỗi tệp không vượt quá 5MB để chống tấn công DoS làm tràn bộ nhớ server hoặc cạn kiệt dung lượng đĩa cứng.
4. **Đổi tên tệp và cách ly đường dẫn (Chống Path Traversal):** Tên file lưu trên ổ cứng được tạo mới hoàn toàn bằng `Guid.NewGuid().ToString("N")` kết hợp đuôi mở rộng hợp lệ. Đường dẫn vật lý được giải quyết an toàn bằng `Path.Combine`, không bao giờ dùng tên gốc người dùng gửi lên để đặt tên file trực tiếp trên hệ điều hành.

---

### Câu 6: Mã QR Code được sinh ra như thế nào? Tại sao không lưu ảnh mã QR vào thư mục `wwwroot` mà lại dùng Data URI Base64?
**Trả lời:**
- Mã QR được sinh động bằng thư viện `QRCoder` đóng gói trong helper `QRCodeHelper.TaoDataUriPng()`.
- Phương thức này nhận vào chuỗi URL chi tiết thiết bị, mã hóa thành các module QR với mức sửa lỗi chất lượng cao (ECCLevel Q), sau đó xuất ra mảng byte PNG và encode thành chuỗi Base64 Data URI dạng `data:image/png;base64,iVBORw0KGgo...`.
- **Lý do không lưu tệp tĩnh vào wwwroot:**
  - Tiết kiệm I/O ổ đĩa: Mỗi lần xem thông tin, mã QR được sinh tức thì trong RAM (với thư viện QRCoder chỉ mất khoảng 1-2 mili-giây cho một mã).
  - Không sinh rác dữ liệu: Nếu lưu tệp ảnh tĩnh, khi sửa thông tin hoặc khi có hàng vạn thiết bị, server sẽ phải lưu trữ và quản lý hàng vạn file ảnh tĩnh không cần thiết.
  - Khi người dùng cần tải ảnh QR về máy tính để in ấn riêng, Controller cung cấp action `/ThietBi/QrCode/{id}` trả về `FileContentResult` dạng tệp tải về `image/png`.

---

### Câu 7: Khi người dùng bấm xóa một thiết bị, hệ thống xử lý ràng buộc toàn vẹn dữ liệu như thế nào?
**Trả lời:**
- Trong `ThietBiService.XoaAsync()`, hệ thống kiểm tra toàn diện dữ liệu liên quan trước khi cho phép xóa:
  - Kiểm tra xem thiết bị đã có kế hoạch bảo trì nào chưa (`KeHoachBaoTris.Any()`).
  - Kiểm tra xem thiết bị đã từng phát sinh báo cáo sự cố nào chưa (`BaoCaoSuCos.Any()`).
- **Quy tắc xử lý:**
  - **Nếu đã có dữ liệu lịch sử:** Hệ thống từ chối xóa và hiển thị thông báo yêu cầu chuyển trạng thái thiết bị sang **"Ngừng sử dụng"** thay vì xóa. Điều này giúp bảo toàn nguyên vẹn lịch sử vận hành, chi phí bảo trì và nhật ký sự cố phục vụ công tác thanh tra/kiểm toán tài sản. Nút "Xóa thiết bị" cũng bị ẩn đi trên giao diện.
  - **Nếu thiết bị mới tạo và chưa phát sinh dữ liệu (nhập nhầm):** Chỉ tài khoản Admin mới có quyền xóa. Khi xóa, hệ thống sẽ tự động dọn dẹp toàn bộ các ảnh thực tế liên quan trong bảng `TepDinhKem` và xóa các file vật lý trong thư mục `wwwroot/uploads`.

---

### Câu 8: Tại sao lại chọn CSS `@media print` với kích thước 80mm x 50mm cho tính năng In tem nhãn tài sản?
**Trả lời:**
- Khổ tem 80mm x 50mm là tiêu chuẩn phổ biến nhất hiện nay trên các dòng máy in tem nhãn nhiệt mã vạch công nghiệp (như Xprinter, Bixolon, Zebra, Godex).
- Khi người dùng xem trên màn hình máy tính, tem nhãn 80x50mm rất nhỏ khó đọc, nên em dùng CSS Transform `scale(1.8)` và responsive để tem to rõ, dễ kiểm tra thông tin.
- Khi người dùng ra lệnh in (Ctrl+P hoặc bấm nút "In ngay"):
  - Directive `@media print` sẽ tự động ẩn toàn bộ thanh công cụ điều hướng (`display: none !important`).
  - Khung tem nhãn được reset `transform: scale(1) !important` về đúng tỷ lệ 1:1.
  - Khai báo `@page { size: 80mm 50mm; margin: 0; }` giúp trình duyệt truyền lệnh chính xác tới driver máy in nhiệt để in vừa khít cuộn tem decal, không bị ngắt trang hay thừa lề trắng.
