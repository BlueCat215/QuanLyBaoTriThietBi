# BÁO CÁO TỰ MÔ TẢ KỸ THUẬT & BỘ CÂU HỎI PHẢN BIỆN BẢO VỆ ĐỒ ÁN
## PHÂN HỆ: MODULE 5 - CHI TIẾT XỬ LÝ, NGHIỆM THU, KHO VẬT TƯ & BÁO CÁO THỐNG KÊ

---

- **Họ và tên sinh viên:** Nguyễn Khắc Hiếu
- **Mã sinh viên:** 23103100225
- **Lớp:** DHTI17A4HN
- **Nhóm:** UNETI01
- **Đề tài:** Hệ thống Quản lý và Bảo trì Thiết bị (MaintainPro)
- **Công nghệ áp dụng:** ASP.NET Core 10 MVC, Entity Framework Core 10, Microsoft SQL Server, ClosedXML, C# 13
- **Phân hệ đảm nhiệm:** Module 5 (Chi tiết xử lý bảo trì/sửa chữa, Nghiệm thu phiếu, Quản lý kho vật tư & kiểm soát tồn kho, Lịch sử vòng đời thiết bị, Dashboard chỉ số vận hành, Báo cáo phân tích thống kê đa chiều theo LINQ, Hệ thống Thông báo nội bộ In-App & Hạ tầng xuất báo cáo Excel đa Sheet)

---

## MỤC I: BÁO CÁO TỰ MÔ TẢ & GIẢI THÍCH KỸ THUẬT

### 1. Tổng quan nghiệp vụ Module 5 đảm nhiệm trong toàn bộ dự án

Module 5 đóng vai trò là **"Tầng thực thi chuyên sâu, Hoàn tất chu trình & Phân tích quản trị" (Execution, Fulfillment & Business Intelligence Layer)** của toàn bộ hệ sinh thái MaintainPro. Sau khi thiết bị được đăng ký (Module 2), lập kế hoạch bảo trì định kỳ (Module 3) và phát sinh báo cáo sự cố hoặc phân công kỹ thuật viên (Module 4), toàn bộ công việc hiện trường, thanh quyết toán vật tư, kiểm định chất lượng và báo cáo chiến lược đều được thực thi tại Module 5:

1. **Thực thi Hiện trường & Quản lý Chi tiết xử lý (`ChiTietXuLy`):**
   - Kỹ thuật viên (KTV) tiếp nhận phiếu và kích hoạt thao tác **Bắt đầu xử lý**, hệ thống tự động đóng dấu `NgayBatDau` và chuyển trạng thái phiếu từ `ChoXuLy` sang `DangXuLy`.
   - Trong quá trình sửa chữa, KTV ghi chép từng hạng mục công việc đã thực hiện, kèm số lượng vật tư, linh kiện thay thế và đơn giá.
   - Cơ chế bảo mật và phân quyền chặt chẽ: Chỉ có KTV đang được phân công hiệu lực (`DangHieuLuc == true`) trên phiếu hoặc Quản trị viên (`Admin`) mới có quyền bắt đầu xử lý, ghi chép hoặc chỉnh sửa dòng chi tiết.

2. **Quy trình Kiểm định & Nghiệm thu 2 bước (Verification & State Machine Transition):**
   - **Bước 1 (KTV hoàn tất công việc):** Sau khi thực hiện xong các hạng mục, KTV nhập tổng kết `KetQuaXuLy` (bắt buộc) và gửi yêu cầu **Chuyển Chờ nghiệm thu**. Phiếu chuyển sang trạng thái `ChoNghiemThu` và đóng quyền thêm/sửa chi tiết.
   - **Bước 2 (Admin nghiệm thu chất lượng):** Chỉ `Admin` mới có thẩm quyền nghiệm thu.
     - Nếu **Đạt**: Phiếu chuyển sang `HoanThanh`, tự động ghi nhận `NgayHoanThanh = DateTime.Now`, tổng hợp chi phí thực tế và đưa thiết bị về trạng thái `HoatDongBinhThuong`.
     - Nếu **Chưa đạt**: Admin nhập lý do yêu cầu khắc phục, phiếu tự động bị đẩy ngược về trạng thái `DangXuLy` để KTV tiếp tục xử lý lại.

3. **Quản lý Danh mục Kho vật tư & Tự động điều tiết tồn kho (`VatTu`):**
   - Quản lý danh mục linh kiện, vật tư thay thế kèm đơn vị tính, đơn giá mặc định và ngưỡng cảnh báo tồn kho tối thiểu (`MucTonToiThieu`).
   - Tự động trừ kho khi KTV xuất vật tư vào bước xử lý, tự động bù/trừ độ lệch khi sửa số lượng, và tự động hoàn trả số lượng vào kho khi xóa dòng xử lý.
   - Áp dụng kỹ thuật **Snapshot Pattern** (`VatTuLinhKien` dạng string kết hợp FK `MaVatTu` nullable) nhằm bảo toàn 100% bằng chứng lịch sử sửa chữa kể cả khi vật tư gốc sau này bị đổi tên hoặc ngừng cung cấp.

4. **Lịch sử vòng đời thiết bị xuyên suốt (`LichSuThietBi` - Timeline 360 độ):**
   - Tổng hợp toàn bộ dữ liệu lịch sử của thiết bị từ ngày đưa vào sử dụng: Kế hoạch bảo trì định kỳ đã lập, các sự cố hư hỏng đã báo cáo, các phiếu xử lý đã triển khai, danh sách tất cả các KTV từng phụ trách và tổng chi phí lũy kế theo thời gian thực. Sắp xếp dòng thời gian theo thứ tự giảm dần.

5. **Bảng điều khiển Chỉ số Vận hành Trung tâm (`Dashboard` - 8 KPIs):**
   - Cung cấp góc nhìn thời gian thực cho Ban Quản trị: Tổng thiết bị & phân loại theo tình trạng hoạt động; Cảnh báo thiết bị sắp đến hạn hoặc quá hạn bảo trì; Số lượng kế hoạch chờ triển khai; Thống kê sự cố theo mức độ nghiêm trọng; Trạng thái các phiếu xử lý; và Tổng chi phí bảo trì thực tế theo từng tháng/năm.

6. **Báo cáo Phân tích Thống kê Đa chiều (`ThongKe` - Multi-Dimensional LINQ Analytics):**
   - Phân tích chi phí và tần suất sự cố theo Loại thiết bị, theo Khu vực lắp đặt, theo Mức độ nghiêm trọng, và đánh giá Hiệu suất hoàn thành công việc của từng Kỹ thuật viên.
   - Trực quan hóa biểu đồ tài chính 12 tháng liên tục và bảng xếp hạng Top thiết bị hỏng nhiều nhất / tiêu tốn chi phí lớn nhất.

7. **Hệ thống Thông báo Nội bộ đa kênh (`ThongBao` - In-App Notification Bell & AJAX):**
   - Phát thông báo tức thời tới tài khoản KTV và Admin khi có sự kiện: phân công việc mới, đổi kỹ thuật viên, phiếu được nghiệm thu hoàn thành hoặc bị trả về. Hỗ trợ badge chuông đỏ đếm số chưa đọc và dropdown xem nhanh qua AJAX không tải lại trang.

8. **Hạ tầng Xuất Báo cáo Excel Đa bảng tính (`ClosedXML Excel Export Engine`):**
   - Thiết kế mô hình xuất dữ liệu Generic hướng đối tượng `ExcelExportHelper` hỗ trợ định dạng chuyên nghiệp: Tiêu đề báo cáo, thông tin người xuất và thời gian, Header styled màu xanh chuẩn nhận diện thương hiệu, căn lề tự động, format số tiền `#,##0 VNĐ`, tự sinh cột STT và hỗ trợ gom nhiều bảng phân tích vào nhiều Sheet (`Worksheet`) trong cùng một tệp `.xlsx`.

---

### 2. Mô tả kiến trúc 3 lớp (3-Tier Architecture) và luồng dữ liệu

Phân hệ Module 5 tuân thủ nguyên lý thiết kế **Tách biệt mối quan tâm (Separation of Concerns - SoC)** và mô hình kiến trúc chuẩn trong ASP.NET Core:

```
[ Trình duyệt Client / View Razor / AJAX Polling ]
               │  ▲
               │  │  HTTP POST / GET / AJAX JSON Request
               ▼  │
[ Controller Layer (Thin Controllers) ]
   - ChiTietXuLyController, VatTuController, ThongKeController, DashboardController, ThongBaoController
   - Trách nhiệm: Xác thực vai trò [Authorize], binding ViewModel, kiểm tra ModelState,
                  xác định người dùng hiện tại (MaTaiKhoan, IsAdmin), điều phối Service.
               │  ▲
               │  │  ViewModel / DTO / ServiceResult<T> / FileStream
               ▼  │
[ Service Layer (Business Logic Engine) ]
   - ChiTietXuLyService, VatTuService, ThongKeService, DashboardService, ThongBaoService
   - Trách nhiệm: State machine chuyển trạng thái phiếu, phân quyền KTV xử lý,
                  tính toán trừ/hoàn kho vật tư an toàn, chụp snapshot tên linh kiện,
                  tính Thành tiền tự động = SL * Đơn giá, phân tích LINQ GroupBy đa chiều,
                  gọi ExcelExportHelper đóng gói file binary.
               │  ▲
               │  │  Entities, Entity Tracking, LINQ Expressions
               ▼  │
[ Repository Layer (Data Access & Query Optimization) ]
   - ChiTietXuLyRepository, VatTuRepository, ThongKeRepository, DashboardRepository, ThongBaoRepository
   - Trách nhiệm: Truy vấn EF Core bất đồng bộ, AsNoTracking() cho tác vụ đọc/thống kê,
                  Include liên kết (PhieuXuLy, ThietBi, VatTu, PhanCongXuLy, TaiKhoan),
                  Query Tracking cho tác vụ cập nhật tồn kho, SaveChangesAsync.
               │  ▲
               │  │  T-SQL Queries, Transactions, Check Constraints
               ▼  │
[ Database Layer: Microsoft SQL Server (MaintainPro DB) ]
```

#### Chi tiết các điểm cốt lõi trong kiến trúc:
- **Tầng Controllers:** Áp dụng mô hình "Thin Controller", tuyệt đối không chứa logic tính toán số tiền, không truy vấn `AppDbContext` trực tiếp. Mọi phản hồi đều đi qua cấu trúc chuẩn `ServiceResult` (hoặc `ServiceResult<T>`), giúp controller chuyển hướng hoặc trả mã lỗi HTTP một cách nhất quán.
- **Tầng Services:** Đóng vai trò hạt nhân kiểm soát nghiệp vụ (Business Rules Guard). Nơi đây kiểm soát toàn bộ ràng buộc: không cho sửa chi tiết khi phiếu không ở trạng thái `DangXuLy`, chặn thao tác nếu số lượng vật tư trong kho không đủ xuất, tự động cập nhật tổng chi phí và chuyển trạng thái thiết bị.
- **Tầng Repositories:** Đảm bảo tối ưu hóa đường truyền dữ liệu bằng việc kích hoạt triệt để `AsNoTracking()` cho các truy vấn hiển thị Timeline, Dashboard và Thống kê. Chỉ kích hoạt Tracking trên các bản ghi Entity cần cập nhật trạng thái hoặc biến động số lượng tồn kho.

---

### 3. Thiết kế Cơ sở dữ liệu và chi tiết các Entity

#### 3.1. Entity `ChiTietXuLy` (Hạt nhân thực thi và quyết toán vật tư)
- **Mã chi tiết (`MaChiTiet`):** Khóa chính tự tăng (Identity, int).
- **Mã phiếu xử lý (`MaPhieuXuLy`):** Khóa ngoại bắt buộc liên kết bảng `PhieuXuLy` (Cascade Delete khi phiếu bị xóa ở trạng thái nháp).
- **Thời điểm thực hiện (`NgayCapNhat`):** Kiểu `DateTime`, mặc định `DateTime.Now`, ghi nhận chính xác thời điểm KTV triển khai công việc.
- **Nội dung thực hiện (`NoiDungThucHien`):** `nvarchar(1000)`, mô tả chi tiết thao tác kỹ thuật (ví dụ: "Tháo động cơ, vệ sinh bạc đạn và thay gioăng cao su chịu nhiệt").
- **Liên kết kho vật tư (`MaVatTu`):** Khóa ngoại `int?` (nullable) trỏ đến `VatTu.MaVatTu`. Quy tắc: `ON DELETE SET NULL`.
- **Bảo toàn lịch sử (`VatTuLinhKien` - Snapshot Pattern):** `nvarchar(200)` cho phép `NULL`. Khi KTV chọn vật tư từ danh mục kho, hệ thống tự động lưu tên vật tư tại thời điểm đó vào cột này. Kể cả sau này danh mục kho đổi tên hoặc xóa vật tư, hóa đơn chi tiết phiếu vẫn giữ nguyên tên gốc.
- **Số lượng (`SoLuong`):** `int`, mặc định 1. Ràng buộc Check Constraint trong CSDL: `[SoLuong] > 0`.
- **Đơn giá (`DonGia`):** `decimal(18,2)`, mặc định 0. Ràng buộc Check Constraint: `[DonGia] >= 0`.
- **Thành tiền (`ThanhTien`):** `decimal(18,2)`. Ràng buộc Check Constraint chặt chẽ tại CSDL:
  ```sql
  CONSTRAINT CK_ChiTiet_ThanhTien CHECK ([ThanhTien] = [SoLuong] * [DonGia])
  ```
  Trường này được tính toán hoàn toàn tự động bởi Service, form nhập liệu trên giao diện bị khóa (readonly), ngăn chặn triệt để tình trạng người dùng cố tình can thiệp sửa đổi giá trị thành tiền.
- **Ghi chú (`GhiChu`):** `nvarchar(500)`, ghi nhận các lưu ý kỹ thuật phát sinh.

#### 3.2. Entity `VatTu` (Quản lý Danh mục Kho vật tư)
- **Mã vật tư (`MaVatTu`):** Khóa chính tự tăng (int).
- **Tên vật tư (`TenVatTu`):** `nvarchar(200)`, bắt buộc. Có Unique Index để tránh trùng tên linh kiện trong kho.
- **Đơn vị tính (`DonViTinh`):** `nvarchar(50)`, ví dụ: "Bộ", "Chiếc", "Lít", "Mét", "Kg".
- **Số lượng tồn (`SoLuongTon`):** `int`, mặc định 0. Ràng buộc CSDL: `[SoLuongTon] >= 0`. Ngăn chặn hoàn toàn lỗi xuất âm kho ở tầng vật lý CSDL.
- **Đơn giá mặc định (`DonGiaMacDinh`):** `decimal(18,2)`, giá tham chiếu khi KTV chọn vật tư thay thế.
- **Ngưỡng cảnh báo tồn kho (`MucTonToiThieu`):** `int`, mặc định 5. Dùng làm căn cứ sinh cảnh báo tự động trên giao diện khi vật tư chạm đáy.
- **Trạng thái (`TrangThai`):** `bool` (`true` = Đang sử dụng, `false` = Ngừng cung cấp). Áp dụng Soft Delete để bảo toàn liên kết khóa ngoại.

#### 3.3. Entity `ThongBao` (Hệ thống Thông báo Nội bộ đa kênh)
- **Mã thông báo (`MaThongBao`):** Khóa chính tự tăng (int).
- **Người nhận (`MaTaiKhoanNhan`):** FK nullable trỏ tới `TaiKhoan`. Nếu `NULL` = Thông báo phát thanh toàn hệ thống (Broadcast).
- **Nội dung:** `TieuDe` (nvarchar 200), `NoiDung` (nvarchar max), `LienKet` (nvarchar 500 - URL điều hướng trực tiếp tới phiếu hoặc sự cố liên quan).
- **Kênh phát (`Kenh`):** Enum `KenhThongBaoEnum` (`InApp`, `Email`, `CaHai`).
- **Thời gian & Trạng thái:** `ThoiGianTao` (`DateTime.Now`), `DaDoc` (`bool`, mặc định `false`), `ThoiGianDoc` (`DateTime?`).

#### 3.4. Quan hệ toàn vẹn & Thuộc tính tính toán động `[NotMapped]`
- **Quan hệ giữa `PhieuXuLy` và `ChiTietXuLy`:** Quan hệ 1 - N. Một phiếu xử lý có thể có nhiều dòng chi tiết công việc và vật tư thay thế.
- **Thuộc tính tính toán động `TongChiPhi`:** Trên Entity `PhieuXuLy`, tổng chi phí không được lưu cứng thành một cột thô độc lập nhằm loại bỏ dư thừa dữ liệu (Data Redundancy) và tránh rủi ro lệch số liệu (Data Inconsistency). Thay vào đó, tổng chi phí được tính toán động:
  ```csharp
  [NotMapped]
  public decimal TongChiPhi => ChiTietXuLys?.Sum(c => c.ThanhTien) ?? 0;
  ```
  Mọi báo cáo, thống kê và màn hình nghiệm thu đều lấy nguồn dữ liệu duy nhất (Single Source of Truth) từ tổng các dòng `ChiTietXuLy.ThanhTien`.

---

### 4. Cơ chế Tìm kiếm, Lọc đa tiêu chí, Phân trang Server-Side & Bảo lưu Query String

Trong các phân hệ của Module 5 (đặc biệt là Danh mục Kho vật tư và Báo cáo thống kê), việc xử lý dữ liệu được thiết kế tối ưu cho môi trường dữ liệu lớn:

1. **Lọc đa tiêu chí & Tìm kiếm toàn văn linh hoạt (Dynamic IQueryable Composition):**
   - Không nạp toàn bộ bảng dữ liệu vào RAM. Truy vấn được xây dựng trên `IQueryable<VatTu>` và chỉ gửi lệnh SQL xuống server khi thực sự cần:
   ```csharp
   var query = _repo.QueryNoTracking();

   if (!string.IsNullOrWhiteSpace(filter.TuKhoa))
   {
       var kw = filter.TuKhoa.Trim().ToLower();
       query = query.Where(v => v.TenVatTu.ToLower().Contains(kw) || 
                                (v.DonViTinh != null && v.DonViTinh.ToLower().Contains(kw)));
   }

   if (filter.CanhBaoTon.HasValue && filter.CanhBaoTon.Value)
   {
       query = query.Where(v => v.SoLuongTon <= v.MucTonToiThieu);
   }
   ```
2. **Phân trang Server-Side hiệu năng cao (`Skip` / `Take`):**
   - Đếm tổng số bản ghi thỏa mãn điều kiện bằng `await query.CountAsync()`.
   - Thực thi phân trang ngay tại tầng SQL Server:
   ```csharp
   var items = await query
       .OrderBy(v => v.TenVatTu)
       .Skip((filter.Trang - 1) * filter.SoBanGhiMoiTrang)
       .Take(filter.SoBanGhiMoiTrang)
       .ToListAsync();
   ```
3. **Bảo lưu trạng thái bộ lọc trên Query String:**
   - Trên Razor View, các nút chuyển trang, các form tìm kiếm và nút xuất Excel đều bind kèm `filter.TuKhoa`, `filter.TrangThai`, `filter.CanhBaoTon`. Nhờ đó, khi người dùng chuyển sang Trang 2 hoặc bấm "Xuất Excel", hệ thống vẫn giữ nguyên phạm vi lọc mà người dùng đang thao tác.

---

### 5. Giải thích các tính năng kỹ thuật chuyên sâu & nâng cao

#### 5.1. Máy trạng thái (State Machine) của Phiếu xử lý & Kiểm soát vòng đời thiết bị
Chu trình xử lý phiếu tuân thủ một đồ thị chuyển trạng thái hữu hạn một chiều, được bảo vệ nghiêm ngặt:
- `ChoXuLy` ➔ (KTV bấm *Bắt đầu xử lý*) ➔ `DangXuLy`: Thiết bị chuyển sang trạng thái bảo trì/sửa chữa, ghi nhận `NgayBatDau`.
- `DangXuLy` ➔ (KTV nhập kết quả & bấm *Chuyển nghiệm thu*) ➔ `ChoNghiemThu`: Khóa chức năng chỉnh sửa chi tiết của KTV để chờ đánh giá.
- `ChoNghiemThu` ➔ (Admin nghiệm thu *Chưa đạt*) ➔ `DangXuLy`: Trả phiếu lại cho KTV kèm nhận xét yêu cầu sửa chữa lại.
- `ChoNghiemThu` ➔ (Admin nghiệm thu *Đạt*) ➔ `HoanThanh`: Đóng phiếu vĩnh viễn, chốt `NgayHoanThanh`, hoàn tất tổng chi phí và đưa thiết bị trở về `HoatDongBinhThuong`.

#### 5.2. Thuật toán Quản lý Xuất/Hoàn kho vật tư an toàn (Safe Inventory Transaction)
Logic xử lý kho được quản lý tập trung tại `ChiTietXuLyService`:
- **Khi thêm mới chi tiết có dùng vật tư:** Kiểm tra `VatTu.SoLuongTon >= SoLuongYeuCau`. Nếu không đủ, hủy giao dịch và thông báo lỗi. Nếu đủ, thực hiện trừ tồn kho: `SoLuongTon -= SoLuong`.
- **Khi cập nhật dòng chi tiết:**
  - Nếu đổi sang loại vật tư khác: Hoàn lại số lượng cũ cho vật tư ban đầu, kiểm tra và trừ kho cho vật tư mới.
  - Nếu giữ nguyên vật tư nhưng đổi số lượng: Tính toán độ lệch `delta = SoLuongMoi - SoLuongCu`. Nếu `delta > 0`, kiểm tra kho còn đủ `delta` không rồi mới trừ tiếp; nếu `delta < 0`, tự động hoàn lại lượng dôi dư về kho.
- **Khi xóa dòng chi tiết:** Tự động hoàn trả nguyên vẹn số lượng vật tư đã sử dụng về lại kho: `SoLuongTon += SoLuong`.

#### 5.3. Báo cáo Thống kê Đa chiều bằng LINQ tối ưu (Multi-Dimensional Aggregation)
Hệ thống tận dụng sức mạnh của LINQ to Objects kết hợp truy vấn bất đồng bộ không theo dõi (`AsNoTracking`) để tính toán đa chiều trong một lần tải dữ liệu:
- Nhóm chi phí theo 12 tháng liên tục của năm tài chính (tự động bù các tháng không phát sinh chi phí bằng 0 để vẽ biểu đồ liền mạch).
- GroupBy theo Loại thiết bị, Khu vực lắp đặt, Mức độ sự cố để tính tỷ trọng chi phí và số lượng sự cố phát sinh.
- Thống kê năng suất KTV: Số phiếu được giao, số phiếu hoàn thành đúng hạn, số phiếu bị nghiệm thu chưa đạt và tổng chi phí đã thực hiện.
- Truy vấn Top 5 thiết bị gặp sự cố nhiều nhất và Top 5 thiết bị ngốn chi phí bảo dưỡng cao nhất.

#### 5.4. Kiến trúc Xuất Excel Đa Bảng Tính (ClosedXML Multi-Sheet Architecture)
Thay vì viết mã xuất Excel lặp lại ở từng Controller, hệ thống xây dựng lớp dùng chung `ExcelExportHelper` với mô hình Generic:
- Định nghĩa danh sách cột bằng `ExcelColumn<T>` với lambda expression `ValueSelector = x => x.Property`.
- Cung cấp phương thức `AddWorksheet<T>()` cho phép một `XLWorkbook` chứa nhiều Sheet riêng biệt (ví dụ: Báo cáo Thống kê xuất ra 6 Sheet: *ChiPhi_12Thang*, *Theo_LoaiThietBi*, *Theo_KhuVuc*, *HieuSuat_KTV*, *Top_ThietBiSuCo*, *Top_ThietBiChiPhi*).
- Tự động áp dụng chuẩn tạo dáng bảng biểu: Banner tên trường/doanh nghiệp, Header màu xanh Cyan đậm (`#004B87`), chữ trắng in đậm, đường viền Thin Border toàn bảng, định dạng số nguyên `#,##0` và định dạng tiền tệ `#,##0 VNĐ`.

---

## MỤC II: BỘ CÂU HỎI & CÂU TRẢ LỜI PHẢN BIỆN BẢO VỆ ĐỒ ÁN

### Câu 1: Tại sao trong bảng `ChiTietXuLy` vừa có khóa ngoại `MaVatTu` liên kết tới bảng `VatTu`, vừa có cột chuỗi `VatTuLinhKien`? Thiết kế này có vi phạm chuẩn hóa dữ liệu không?
**Trả lời:**
Em xin khẳng định đây là một giải pháp thiết kế có chủ đích theo mẫu thiết kế **Snapshot Pattern (Chụp ảnh trạng thái dữ liệu)** rất phổ biến trong các hệ thống Quản lý bảo trì (CMMS) và Thương mại điện tử (E-Commerce), hoàn toàn không phải do thiết kế CSDL sơ sài:
- Khóa ngoại `MaVatTu` (cho phép `NULL`, `ON DELETE SET NULL`) dùng để phục vụ tác vụ nghiệp vụ hiện tại: liên kết kho, kiểm tra tồn kho, trừ kho tự động và thống kê vật tư nào đang được tiêu thụ nhiều nhất.
- Cột `VatTuLinhKien` (chuỗi `nvarchar(200)`) lưu lại đúng tên của vật tư tại chính thời điểm kỹ thuật viên xuất kho thực hiện bảo trì.
- **Ý nghĩa sống còn:** Trong thực tế quản lý, một danh mục vật tư sau 3 - 5 năm có thể bị sửa tên, thay đổi quy cách, hoặc thậm chí bị xóa/ngừng sử dụng do nhà cung cấp ngừng sản xuất. Nếu chúng ta chỉ lưu `MaVatTu` mà không chụp snapshot chuỗi tên, khi vật tư gốc bị xóa hoặc sửa đổi, toàn bộ biên bản sửa chữa trong quá khứ sẽ bị hiển thị sai lệch thông tin hoặc biến thành chuỗi rỗng (`NULL`). Việc kết hợp cả hai vừa đảm bảo tính liên kết quan hệ trong hiện tại, vừa đảm bảo tính toàn vẹn lịch sử bất biến cho hồ sơ pháp lý kỹ thuật của thiết bị.

---

### Câu 2: Logic trừ kho và hoàn kho vật tư được đặt tại Tầng Service (`ChiTietXuLyService`). Tại sao em không dùng Stored Procedure hoặc Database Trigger trong SQL Server để tự động hóa việc này?
**Trả lời:**
Em lựa chọn đặt toàn bộ logic trừ kho/hoàn kho tại Service Layer vì các lý do kỹ thuật sau:
1. **Khả năng kiểm thử (Testability) và Kiểm soát ngoại lệ (Exception Handling):** Khi kiểm tra tồn kho tại Service, nếu số lượng tồn trong kho không đủ, Service có thể trả về một `ServiceResult` thất bại kèm câu thông báo thân thiện rõ ràng (ví dụ: *"Vật tư 'Vòng bi SKF 6204' chỉ còn 2 chiếc trong kho, không đủ xuất 5 chiếc"*). Thông báo này được đưa thẳng lên giao diện qua `ModelState`. Nếu dùng Trigger, việc bắt lỗi SQL Exception và bóc tách thông báo lỗi tiếng Việt thân thiện tới người dùng ở tầng Web sẽ rất phức tạp và thiếu tự nhiên.
2. **Nguyên tắc phân tầng kiến trúc (Single Responsibility & SoC):** Theo chuẩn kiến trúc hiện đại của ASP.NET Core, Database chỉ nên chịu trách nhiệm lưu trữ và bảo đảm các ràng buộc vật lý cơ bản (như Check Constraint `SoLuongTon >= 0`). Toàn bộ quy tắc nghiệp vụ (Business Rules), quy trình luân chuyển dữ liệu và xử lý giao dịch nên được đóng gói tại Tầng Service để dễ dàng bảo trì, nâng cấp và migrate giữa các hệ quản trị CSDL khác nhau nếu cần.
3. **Bảo toàn dữ liệu 2 lớp (Defense-in-depth):** Mặc dù kiểm tra tại Service, em vẫn thiết lập ràng buộc `CONSTRAINT CK_VatTu_SoLuongTon CHECK (SoLuongTon >= 0)` ngay tại bảng SQL Server. Điều này tạo nên hàng rào bảo vệ kép: Service ngăn ngừa lỗi nghiệp vụ, còn Database là chốt chặn cuối cùng ngăn ngừa sai sót dữ liệu.

---

### Câu 3: Làm thế nào em ngăn chặn được tình trạng Race Condition (Tranh chấp tài nguyên) dẫn đến hiện tượng xuất âm kho khi 2 kỹ thuật viên cùng xuất một loại vật tư tại cùng một thời điểm?
**Trả lời:**
Để giải quyết bài toán này, hệ thống của em áp dụng cơ chế kiểm soát đồng thời và khóa dữ liệu:
1. **Ở mức CSDL:** Bảng `VatTu` có ràng buộc kiểm tra Check Constraint `[SoLuongTon] >= 0`. Nếu hai request cùng vượt qua bước kiểm tra tại Service nhưng khi ghi xuống SQL Server, transaction nào ghi trước làm tồn kho giảm xuống dưới 0 thì câu lệnh của request thứ hai sẽ lập tức bị SQL Server ném ra ngoại lệ `DbUpdateException` và giao dịch bị Rollback hoàn toàn, bảo đảm kho không bao giờ bị âm.
2. **Ở tầng EF Core:** Trong phương thức `ThemAsync` và `SuaAsync`, quá trình nạp Entity `VatTu` được thực hiện với cơ chế Tracking trực tiếp trên DbContext hiện hành của Scope Request. Mọi thao tác trừ tồn kho và tạo dòng `ChiTietXuLy` đều được thực thi trong cùng một lần gọi `await _context.SaveChangesAsync()`. EF Core tự động gói các thao tác này vào một `Database Transaction` ngầm định, đảm bảo tính nguyên tử (Atomicity) theo chuẩn ACID: hoặc cả hai thao tác cùng thành công, hoặc không có thao tác nào được lưu.

---

### Câu 4: Tại sao trong bảng `PhieuXuLy` em không tạo một cột `TongChiPhi` để lưu số tiền vào CSDL, mà lại sử dụng thuộc tính `[NotMapped]` tính tổng từ `ChiTietXuLy`? Làm như vậy khi hiển thị danh sách phiếu có bị chậm (N+1 Query) không?
**Trả lời:**
1. **Lý do không lưu cứng cột `TongChiPhi`:**
   - Trong thiết kế CSDL quan hệ chuẩn (3NF), việc lưu trữ một giá trị hoàn toàn có thể tính toán suy dẫn được từ các bảng chi tiết con được gọi là dư thừa dữ liệu (Data Redundancy).
   - Nếu lưu cứng `TongChiPhi` trên `PhieuXuLy`, mỗi khi người dùng Thêm, Sửa số lượng, Sửa đơn giá hoặc Xóa một dòng `ChiTietXuLy`, hệ thống bắt buộc phải nhớ cập nhật lại cột `TongChiPhi` của phiếu cha. Nếu một lập trình viên khác sau này viết thêm tính năng mà quên cập nhật phiếu cha, dữ liệu giữa phiếu và chi tiết sẽ bị lệch (Inconsistent Data). Việc dùng thuộc tính `[NotMapped]` bảo đảm `ChiTietXuLy` luôn là "Nguồn chân lý duy nhất" (Single Source of Truth).
2. **Giải pháp triệt tiêu N+1 Query khi hiển thị danh sách:**
   - Em không gọi thuộc tính `TongChiPhi` trong một vòng lặp `foreach` khi các dòng chi tiết chưa được nạp.
   - Khi cần hiển thị danh sách phiếu kèm chi phí, trong Repository em sử dụng phép chiếu LINQ `Select()` trực tiếp xuống tầng SQL Server hoặc sử dụng `.Include(p => p.ChiTietXuLys)`. Nhờ đó, SQL Server sẽ thực thi câu lệnh gom nhóm `SUM(ThanhTien)` ngay tại CSDL trong đúng 1 truy vấn duy nhất, hoàn toàn không phát sinh lỗi N+1 Query.

---

### Câu 5: Thành tiền trong `ChiTietXuLy` được tính như thế nào? Người dùng có thể tự gõ số tiền thanh toán theo ý muốn trên giao diện không?
**Trả lời:**
- **Quy tắc tính toán:** Thành tiền tuyệt đối do hệ thống tự động xác định theo công thức:
  $$\text{ThanhTien} = \text{SoLuong} \times \text{DonGia}$$
- **Kiểm soát giao diện:** Trên View `TheoPhieu.cshtml` và các form modal nhập liệu, trường Thành tiền không cho phép người dùng tự do nhập liệu mà được hiển thị dưới dạng nhãn tính toán hoặc ô `readonly/disabled`, tự động cập nhật qua JavaScript khi người dùng thay đổi số lượng hoặc đơn giá để người dùng dễ quan sát trước khi lưu.
- **Bảo mật tầng Server:** Khi form submit lên Controller, Service tuyệt đối không tin tưởng dữ liệu `ThanhTien` gửi lên từ Client (tránh việc kẻ xấu dùng Inspect Element hoặc Postman để sửa số tiền). Service tự động lấy `SoLuong` nhân với `DonGia` đã được validate để gán vào `entity.ThanhTien`.
- **Ràng buộc cứng tại CSDL:** Tại SQL Server, bảng `ChiTietXuLy` được cài đặt Check Constraint: `CHECK ([ThanhTien] = [SoLuong] * [DonGia])`. Nếu có bất kỳ câu lệnh SQL bất hợp pháp nào cố tình gán Thành tiền khác với tích số, SQL Server sẽ chặn đứng ngay lập tức.

---

### Câu 6: Cơ chế phân quyền giữa Kỹ thuật viên và Quản trị viên trong phân hệ Chi tiết xử lý và Nghiệm thu phiếu được thiết kế ra sao?
**Trả lời:**
Hệ thống áp dụng cơ chế bảo mật đa lớp (Multi-layer Authorization):
1. **Phân quyền theo Vai trò (Role-Based Authorization):**
   - Các Action quản lý vật tư (Thêm/Sửa/Xóa/Chuyển trạng thái) và Action `NghiemThu` phiếu được khóa chặt bằng thuộc tính `[Authorize(Roles = AppRoles.Admin)]`. KTV cố tình truy cập sẽ bị chặn với mã lỗi HTTP 403 Forbidden.
   - Các Action nhập chi tiết xử lý được mở cho cả Admin và KTV qua `[Authorize(Roles = AppRoles.AdminOrKyThuatVien)]`.
2. **Phân quyền theo Ngữ cảnh sở hữu (Context-Based / Ownership Authorization):**
   - Một KTV đăng nhập không được phép can thiệp vào phiếu của KTV khác.
   - Khi KTV thực hiện các thao tác: *Bắt đầu xử lý*, *Thêm chi tiết*, *Sửa chi tiết*, *Xóa chi tiết*, hoặc *Chuyển nghiệm thu*, Service sẽ kiểm tra trong bảng `PhanCongXuLy`:
   ```csharp
   var laKtvDangPhuTrach = phieu.PhanCongXuLys
       .Any(pc => pc.MaKyThuatVien == maTaiKhoanDangNhap && pc.DangHieuLuc);
   ```
   Nếu người thao tác không phải là Admin và cũng không phải là KTV đang phụ trách phiếu đó, Service sẽ từ chối xử lý và trả về thông báo lỗi: *"Bạn không có quyền thao tác trên phiếu xử lý này"*.

---

### Câu 7: Bảng điều khiển `Dashboard` và trang `ThongKe` xử lý lượng dữ liệu lớn như thế nào để đảm bảo không làm nghẽn hệ thống?
**Trả lời:**
Để tối ưu hóa hiệu năng cho các màn hình giám sát và thống kê:
1. **Sử dụng `AsNoTracking()` toàn diện:** Toàn bộ các câu truy vấn phục vụ Dashboard và Thống kê đều được gọi qua `QueryNoTracking()` trong Repository. Điều này chỉ thị cho EF Core không kích hoạt bộ theo dõi thay đổi (`Change Tracker`), giúp tiết kiệm đáng kể bộ nhớ RAM và tăng tốc độ đọc từ 30% đến 50%.
2. **Thực thi tính toán bất đồng bộ (`async / await`):** Tất cả các câu truy vấn Count, Sum, ToList đều sử dụng các hàm bất đồng bộ (`CountAsync`, `SumAsync`, `ToListAsync`), giúp giải phóng luồng xử lý (`Thread Pool`) của web server trong thời gian chờ CSDL trả lời, nâng cao khả năng chịu tải đồng thời của ứng dụng.
3. **Khai thác LINQ GroupBy hiệu quả:** Thay vì truy vấn lặp nhiều lần (ví dụ: truy vấn từng loại thiết bị rồi đếm), hệ thống nhóm dữ liệu trực tiếp bằng `GroupBy` (nhóm theo `MaLoaiThietBi`, nhóm theo `MaKhuVuc`, nhóm theo `MucDoSuCo`), cho phép SQL Server tối ưu hóa câu lệnh bằng các chỉ mục có sẵn (`Index`) và trả về kết quả tổng hợp trong thời gian ngắn nhất.

---

### Câu 8: Hãy giải thích thiết kế của lớp dùng chung `ExcelExportHelper` và cách xuất báo cáo thống kê đa Sheet bằng ClosedXML?
**Trả lời:**
Để không phải viết lại mã định dạng Excel rườm rà ở từng tính năng, em đã thiết kế `ExcelExportHelper` theo kiến trúc Generic:
1. **Mô hình ánh xạ cột `ExcelColumn<T>`:** Mỗi cột trong tệp Excel được biểu diễn bằng một đối tượng chứa: `Header` (tiêu đề cột tiếng Việt), `Width` (độ rộng cột), `Alignment` (căn trái, phải, giữa), `Format` (định dạng số `#,#0` hoặc ngày tháng) và `ValueSelector` kiểu delegate `Func<T, object?>` để trích xuất linh hoạt giá trị từ bất kỳ thuộc tính nào của đối tượng nguồn.
2. **Phương thức `AddWorksheet<T>()`:** Nhận vào một đối tượng `XLWorkbook` đang mở, tên Sheet, tiêu đề báo cáo, danh sách dữ liệu `IEnumerable<T>` và danh sách định nghĩa cột. Phương thức này tự động dựng Header, vẽ đường viền bảng tính, định dạng màu sắc nhận diện thương hiệu và điền số liệu kèm cột STT tự tăng.
3. **Cơ chế xuất đa Sheet (Multi-Sheet Export):**
   - Khi xuất báo cáo Thống kê tổng hợp, Service khởi tạo một `new XLWorkbook()`.
   - Lần lượt gọi `AddWorksheet()` cho 6 tập dữ liệu khác nhau: Bảng chi phí 12 tháng, Bảng phân tích theo loại thiết bị, Bảng theo khu vực, Bảng hiệu suất KTV, Top thiết bị sự cố và Top thiết bị chi phí.
   - Cuối cùng, phương thức `SaveToBytes()` đóng gói toàn bộ workbook vào một luồng bộ nhớ `MemoryStream` và trả về mảng `byte[]` sạch cho Controller trả về file dạng `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`.
   - Cách làm này giúp code cực kỳ trong sáng, tái sử dụng tuyệt đối và bảo đảm tính nhất quán giao diện văn bản xuất ra theo chuẩn quy định của doanh nghiệp.
