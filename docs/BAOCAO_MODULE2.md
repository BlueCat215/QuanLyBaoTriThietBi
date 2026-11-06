# BÁO CÁO BÀI TẬP LỚN: MODULE 2 - QUẢN LÝ THIẾT BỊ
**HỌC PHẦN: LẬP TRÌNH .NET (ASP.NET CORE MVC)**  
**Họ và tên sinh viên phụ trách:** Nguyễn Minh Đức  
**Lớp:** TI17A4HN — **Nhóm:** UNETI01  
**Đề tài:** Hệ thống Quản lý và Bảo trì Thiết bị (MaintainPro)  

---

## 1. TỔNG QUAN PHÂN CÔNG MODULE 2 THEO CHECKLIST

| STT | Tuần | Nội dung công việc | Kết quả đạt được | Trạng thái |
| :---: | :---: | :--- | :--- | :---: |
| **5** | Tuần 1 | Viết Entity `ThietBi` + Data Annotations, cấu hình Navigation Properties, comment đầu file. | Entity `ThietBi` hoàn chỉnh, quan hệ với `LoaiThietBi`, `KhuVuc`. | ✅ Hoàn thành |
| **14** | Tuần 2 | Xây dựng CRUD Thiết bị (Index, Details, Create, Edit, DoiTinhTrang, Delete an toàn); Validation nghiệp vụ. | Bộ chức năng CRUD hoàn chỉnh kèm thông báo lỗi/thành công. | ✅ Hoàn thành |
| **15** | Tuần 2 | Xây dựng chức năng Tìm kiếm thiết bị theo Tên / Số serial / Tên khu vực bằng LINQ. | Tìm kiếm đa từ khóa, đa trường dữ liệu trên SQL Server. | ✅ Hoàn thành |
| **27** | Tuần 3 | Xây dựng chức năng Lọc (loại, khu vực, tình trạng, khoảng ngày, hạn bảo trì) và Sắp xếp (8 tiêu chí). | Bộ lọc đa tiêu chí + sắp xếp ổn định. | ✅ Hoàn thành |
| **28** | Tuần 3 | Phân trang bằng `Skip()` / `Take()`; tích hợp SFSP đầy đủ, giữ nguyên điều kiện khi đổi trang. | Phân trang trực tiếp trên SQL Server, bảo toàn query string. | ✅ Hoàn thành |
| **29** | Tuần 3 | LINQ tính `NgayBaoTriTiepTheo = (NgayBaoTriGanNhat ?? NgayDuaVaoSuDung) + ChuKyBaoTriThang`; phân loại Chưa/Sắp/Quá hạn. | Logic tính hạn bảo trì thống nhất toàn hệ thống. | ✅ Hoàn thành |
| **30** | Tuần 3 | Tự review code, chuẩn hóa comment đầu file, hoàn thiện tài liệu báo cáo Module 2. | Tài liệu báo cáo hoàn chỉnh, code sẵn sàng bảo vệ. | ✅ Hoàn thành |
| **53** | Nâng cao | Hệ thống Upload tệp đính kèm đa hình (`TepDinhKem`): Service/Helper upload file an toàn (MIME-type, dung lượng 5MB, GUID chống ghi đè, xóa tệp đĩa). | Hạ tầng upload đa hình dùng chung Module 2 và Module 4. | ✅ Hoàn thành |
| **54** | Nâng cao | Quản lý ảnh thiết bị thực tế: Upload nhiều ảnh trong form Tạo/Sửa và trang Chi tiết; bộ sưu tập Gallery và Lightbox preview. | Gallery ảnh trực quan, phóng to lightbox, upload/xóa nhanh. | ✅ Hoàn thành |
| **56** | Nâng cao | Sinh mã QR động và In tem nhãn thiết bị: Tích hợp thư viện QRCoder, thiết kế giao diện in tem nhãn tài sản dán máy (Print template). | Sinh QR động từ URL, tải ảnh PNG, mẫu in tem 80x50mm. | ✅ Hoàn thành |

---

## 2. THIẾT KẾ CƠ SỞ DỮ LIỆU & ENTITY `ThietBi`

### 2.1. Cấu trúc bảng `ThietBi`
Bảng `ThietBi` là trung tâm của hệ thống, liên kết với danh mục quản trị (`LoaiThietBi`, `KhuVuc`) và là đối tượng tham chiếu của các quy trình nghiệp vụ bảo trì, sự cố:

* **Mã thiết bị (`MaThietBi`):** Khóa chính (PK), tự tăng (`int`).
* **Tên thiết bị (`TenThietBi`):** `nvarchar(150)`, bắt buộc.
* **Số serial (`SoSerial`):** `nvarchar(100)`, bắt buộc, duy nhất (`UNIQUE`).
* **Mã loại thiết bị (`MaLoaiThietBi`):** Khóa ngoại (FK) tham chiếu `LoaiThietBi(MaLoaiThietBi)`.
* **Mã khu vực (`MaKhuVuc`):** Khóa ngoại (FK) tham chiếu `KhuVuc(MaKhuVuc)`.
* **Ngày đưa vào sử dụng (`NgayDuaVaoSuDung`):** `date`, bắt buộc.
* **Chu kỳ bảo trì (`ChuKyBaoTriThang`):** `int`, đơn vị: tháng, giá trị $> 0$.
* **Ngày bảo trì gần nhất (`NgayBaoTriGanNhat`):** `date`, nullable (`NULL` = chưa bảo trì lần nào).
* **Tình trạng (`TinhTrang`):** `nvarchar(30)` (lưu chuỗi Enum: `DangHoatDong`, `DangBaoTri`, `TamNgungDoSuCo`, `NgungSuDung`).
* **Mô tả (`MoTa`):** `nvarchar(255)`, nullable.

### 2.2. Điểm đặc thù theo quy định đề bài (Mục 2.1 THIETKE.md)
* **Tuyệt đối KHÔNG tạo cột `NgayBaoTriTiepTheo` trong DB:** Đây là yêu cầu bắt buộc của đề tài (mục 25). Giá trị này phải được tính động thông qua biểu thức LINQ và thuộc tính `[NotMapped]` trong Entity:
  ```csharp
  [NotMapped, Display(Name = "Ngày bảo trì tiếp theo")]
  public DateTime NgayBaoTriTiepTheo =>
      (NgayBaoTriGanNhat ?? NgayDuaVaoSuDung).AddMonths(ChuKyBaoTriThang);
  ```
* **Tuyệt đối KHÔNG tạo cột `TrangThaiHanBaoTri` trong DB:** Trạng thái Chưa đến hạn / Sắp đến hạn / Quá hạn được suy diễn từ hiệu số ngày giữa `NgayBaoTriTiepTheo` và ngày hiện tại.
* **Tuyệt đối KHÔNG thêm cột lưu mã QR trong DB (Mục 6.3 THIETKE.md):** Mã QR là chuỗi encode từ URL chi tiết hoặc Serial, sinh động dạng Base64 Data URI tại thời điểm render/in.

---

## 3. CÁC RÀNG BUỘC VÀ QUY TẮC NGHIỆP VỤ (BUSINESS RULES)

Toàn bộ quy tắc được kiểm soát đa tầng (Data Annotations ở ViewModel + Kiểm tra logic bất đồng bộ trong Service):

1. **Ràng buộc Số Serial duy nhất (`SoSerial`):**
   - Không được phép trùng lặp trong toàn hệ thống.
   - Khi Thêm mới: kiểm tra `AnyAsync(t => t.SoSerial == vm.SoSerial)`.
   - Khi Cập nhật (Sửa): bỏ qua chính bản ghi đang sửa: `AnyAsync(t => t.SoSerial == vm.SoSerial && t.MaThietBi != id)`.
2. **Ràng buộc Thời gian sử dụng (`NgayDuaVaoSuDung`):**
   - Không được lớn hơn ngày hiện tại (`NgayDuaVaoSuDung <= DateTime.Today`).
   - Phải hợp lệ về mặt lịch sử ($\ge 01/01/1900$).
3. **Ràng buộc Chu kỳ bảo trì (`ChuKyBaoTriThang`):** Phải là số nguyên dương $> 0$ và $\le 600$ tháng.
4. **Ràng buộc Ngày bảo trì gần nhất (`NgayBaoTriGanNhat`):**
   - Không được lớn hơn ngày hiện tại.
   - Không được xảy ra trước ngày đưa vào sử dụng thiết bị (`NgayBaoTriGanNhat >= NgayDuaVaoSuDung`).
5. **Ràng buộc Danh mục liên quan (`LoaiThietBi`, `KhuVuc`):** Bắt buộc phải tồn tại và đang ở trạng thái Hoạt động (`TrangThai == true`).
6. **Quy tắc Xóa an toàn (Safe Delete):**
   - Chỉ cho phép xóa vật lý thiết bị khi chưa phát sinh bất kỳ bản ghi nào ở các bảng liên quan: `KeHoachBaoTri`, `BaoCaoSuCo`, `ThongBao`, `TepDinhKem`.
   - Nếu đã phát sinh dữ liệu: chặn lệnh xóa và hướng dẫn người dùng chuyển sang trạng thái **Ngừng sử dụng** để bảo toàn lịch sử chu trình bảo trì và chi phí.

---

## 4. CHI TIẾT CÁC TÍNH NĂNG NÂNG CAO CỦA MODULE 2

### 4.1. Hệ thống Upload tệp đính kèm đa hình (STT 53)
- Bảng `TepDinhKem` dùng chung toàn hệ thống thông qua cặp khóa ngoại đa hình: `LoaiDoiTuong` (Enum: `ThietBi`, `BaoCaoSuCo`, `ChiTietXuLy`) + `MaDoiTuong` (ID đối tượng cha).
- Tầng Service tự động kiểm tra đối tượng cha tồn tại trước khi Insert (`KiemTraDoiTuongTonTaiAsync`).
- Kiểm duyệt bảo mật:
  - Dung lượng file tối đa: $5\,\text{MB}$.
  - Chặn tuyệt đối mã độc thực thi (`.exe`, `.bat`, `.cmd`, `.sh`, `.php`, `.asp`, `.js`,...).
  - Chỉ chấp nhận ảnh: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`.
  - Tên file lưu trên đĩa sinh ngẫu nhiên bằng `Guid:N` + extension để chống trùng lặp và chống lộ cấu trúc thư mục.
  - Tự động xóa file vật lý trên đĩa cứng khi xóa bản ghi.

### 4.2. Quản lý ảnh thiết bị thực tế (STT 54)
- **Form Thêm mới & Sửa:** Cho phép chọn đính kèm nhiều ảnh hiện trạng thiết bị (`multipart/form-data`). Form Sửa hiển thị trước danh sách ảnh đang có kèm thumbnail và nút xóa nhanh.
- **Trang Chi tiết (`ThietBi/Details`):**
  - **Gallery:** Lưới ảnh thumbnail responsive, hiệu ứng hover phóng to nhẹ.
  - **Lightbox Modal:** Bấm vào bất kỳ ảnh nào để xem phóng to sắc nét, hiển thị tên file gốc, thời gian tải lên, người tải và nút tải ảnh gốc.
  - **Upload nhanh:** Nút "Tải thêm ảnh" mở modal cho phép tải thêm ảnh trực tiếp ngay tại trang Details mà không cần rời trang.
  - **Xóa ảnh:** Nút xóa ảnh có hộp thoại xác nhận `confirm`, kiểm soát phân quyền KTV/Admin.

### 4.3. Sinh mã QR động và In tem nhãn thiết bị (STT 56)
- **Nguyên tắc:** TUYỆT ĐỐI KHÔNG thêm cột mã QR vào cơ sở dữ liệu.
- Sử dụng thư viện `QRCoder` sinh trực tiếp Data URI Base64 PNG:
  ```csharp
  vm.QrCodeDataUri = QRCodeHelper.TaoDataUriPng(urlChiTiet, pixelsPerModule: 5);
  ```
- **Màn hình In tem nhãn tài sản dán máy (`/ThietBi/InTemNhan/{id}`):**
  - Định dạng chuẩn kích thước dán hiện trường ($80\,\text{mm} \times 50\,\text{mm}$).
  - Gồm: Mã thiết bị, tên máy, số Serial màu đỏ nổi bật font monospace, khu vực, ngày bảo trì tiếp theo và mã QR động.
  - CSS `@media print` tự động căn lề không viền và ẩn thanh công cụ khi in ra máy in nhiệt/laser.
  - Endpoint `/ThietBi/QrCode/{id}` cho phép tải trực tiếp file ảnh `QR_{Serial}_{Id}.png`.

---

## 5. THUẬT TOÁN SFSP (SEARCH - FILTER - SORT - PAGINATION)

### 5.1. Thứ tự thực thi chuẩn trên SQL Server
Luôn tuân thủ nghiêm ngặt thứ tự xử lý IQueryable:
$$\text{Where (Search + Filter)} \longrightarrow \text{OrderBy (Sort)} \longrightarrow \text{Skip \& Take (Pagination)}$$

1. **Tìm kiếm (Search):** Tách từ khóa thành các token (tối đa 5 từ), áp dụng `AND` giữa các từ và `OR` giữa 3 cột: `TenThietBi`, `SoSerial`, `KhuVuc.TenKhuVuc`.
2. **Lọc (Filter):**
   - Loại thiết bị (`MaLoaiThietBi`)
   - Khu vực (`MaKhuVuc`)
   - Tình trạng (`TinhTrang`)
   - Khoảng ngày đưa vào sử dụng (`NgaySuDungTu` $\le$ Ngày $\le$ `NgaySuDungDen`)
   - Hạn bảo trì (`TrangThaiHanBaoTriEnum`: Quá hạn, Sắp đến hạn, Chưa đến hạn)
3. **Sắp xếp (Sort):** 8 tiêu chí sắp xếp kết hợp khóa phụ `ThenBy(t => t.MaThietBi)` để đảm bảo tính tất định (deterministic) khi phân trang.
4. **Phân trang (Pagination):** Thực hiện trực tiếp trên database bằng `CountAsync()` lấy tổng số bản ghi và `Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync()`.

### 5.2. Công thức tính hạn bảo trì thống nhất
- **Hạn bảo trì:**
  $$\text{NgayBaoTriTiepTheo} = (\text{NgayBaoTriGanNhat} \ ?? \ \text{NgayDuaVaoSuDung}) + \text{ChuKyBaoTriThang} \ (\text{tháng})$$
- **Phân loại trạng thái (Ngưỡng cảnh báo: 7 ngày):**
  - **Quá hạn:** $\text{NgayBaoTriTiepTheo} < \text{Hôm nay}$
  - **Sắp đến hạn:** $\text{Hôm nay} \le \text{NgayBaoTriTiepTheo} < \text{Hôm nay} + 8 \ \text{ngày}$
  - **Chưa đến hạn:** $\text{NgayBaoTriTiepTheo} \ge \text{Hôm nay} + 8 \ \text{ngày}$
  - *Lưu ý:* Thiết bị ở tình trạng `NgungSuDung` không tính hạn bảo trì.

---

## 6. BỘ CÂU HỎI VÀ TRẢ LỜI PHẢN BIỆN BẢO VỆ ĐỒ ÁN

### Câu 1: Tại sao không tạo luôn cột `NgayBaoTriTiepTheo` và `QrCode` trong SQL Server để truy vấn cho nhanh?
* **Trả lời:**
  - **Tuân thủ chuẩn thiết kế 3NF & Quy định đề bài:** Mục 25 của đề bài và mục 2.1 tài liệu thiết kế cấm tạo cột này vì đây là trường dẫn xuất (derived field).
  - **Tránh sai lệch dữ liệu:** Nếu lưu cột, mỗi khi người dùng đổi ngày sử dụng, chu kỳ bảo trì hoặc cập nhật ngày bảo trì gần nhất, dữ liệu rất dễ bị bất đồng bộ.
  - **Mã QR là ảnh sinh từ URL/Serial:** Sinh động dạng Base64 Data URI không tốn bộ nhớ lưu trữ DB và không lo lỗi thời khi Serial thay đổi.
  - **Hiệu năng:** SQL Server hỗ trợ hàm `DATEADD` kết hợp `COALESCE` cực nhanh; việc tính động đảm bảo dữ liệu luôn chính xác 100% theo thời gian thực.

### Câu 2: Làm thế nào để đảm bảo người dùng đang ở trang 3 khi lọc dữ liệu không bị rơi vào trang trắng?
* **Trả lời:**
  - Trong form lọc, chúng tôi đặt thẻ `<input type="hidden" name="Trang" value="1" />`. Mỗi khi bấm "Áp dụng lọc", hệ thống chủ động đưa về trang 1.
  - Trong phương thức `PagedResult<T>.CreateAsync()`, có cơ chế tự động nắn: nếu `page > totalPages`, hệ thống tự động gán `page = totalPages`.

### Câu 3: Làm thế nào ngăn chặn người dùng xóa mất lịch sử bảo trì của một thiết bị?
* **Trả lời:**
  - Áp dụng nguyên tắc **Xóa an toàn (Safe Delete)**. Trước khi xóa, hàm `DemDuLieuLienQuanAsync` đếm bất đồng bộ số lượng liên kết ở 4 bảng: `KeHoachBaoTri`, `BaoCaoSuCo`, `ThongBao`, `TepDinhKem`.
  - Nếu tổng số bản ghi $> 0$, hệ thống chặn lệnh xóa, trả về thông báo lỗi thân thiện và hướng dẫn chuyển sang tình trạng `NgungSuDung`.

### Câu 4: Làm sao để giữ nguyên các điều kiện lọc và sắp xếp khi người dùng bấm chuyển trang?
* **Trả lời:**
  - Lớp `ThietBiBoLoc` có phương thức `ToRouteValues(int? trang)` đóng gói toàn bộ thuộc tính (`TuKhoa`, `MaLoaiThietBi`, `MaKhuVuc`, `TinhTrang`, `HanBaoTri`, `NgaySuDungTu`, `NgaySuDungDen`, `SapXep`, `SoBanGhiMoiTrang`) vào `Dictionary<string, string>`.
  - Trên View Razor, link phân trang dùng `asp-all-route-data="@bl.ToRouteValues(pageIndex)"` giúp bảo toàn 100% URL parameters.

### Câu 5: Hệ thống Upload tệp đính kèm đa hình (STT 53) giải quyết vấn đề khóa ngoại như thế nào?
* **Trả lời:**
  - Vì bảng `TepDinhKem` dùng chung cho nhiều bảng cha (`ThietBi`, `BaoCaoSuCo`, `ChiTietXuLy`), trong SQL Server không thể tạo Foreign Key vật lý.
  - Ràng buộc được bảo đảm tuyệt đối ở **tầng Service**: `KiemTraDoiTuongTonTaiAsync` sẽ kiểm tra đối tượng cha tương ứng trước khi cho phép insert. Đồng thời các file nguy hiểm (`.exe`, `.bat`, `.sh`...) bị chặn hoàn toàn và tên file được băm bằng `GUID` chống trùng lặp.

---
**Xác nhận của sinh viên thực hiện:**  
*Nguyễn Minh Đức (DHTI17A4HN) — Đã hoàn thành 100% khối lượng công việc Module 2 (STT 5, 14, 15, 27, 28, 29, 30, 53, 54, 56) và sẵn sàng bảo vệ.*
