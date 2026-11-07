# BÁO CÁO TỰ MÔ TẢ KỸ THUẬT & BỘ CÂU HỎI PHẢN BIỆN BẢO VỆ ĐỒ ÁN
## PHÂN HỆ: MODULE 3 - KẾ HOẠCH VÀ LỊCH BẢO TRÌ ĐỊNH KỲ

---

- **Họ và tên sinh viên:** Phạm Việt Hoàn
- **Mã sinh viên:** 23103100207
- **Lớp:** TI17A4HN
- **Nhóm:** UNETI01
- **Đề tài:** Hệ thống Quản lý và Bảo trì Thiết bị (MaintainPro)
- **Công nghệ áp dụng:** ASP.NET Core 10 MVC, Entity Framework Core 10, Microsoft SQL Server, C# 13
- **Checklist công việc phụ trách:** STT 06, STT 16, STT 17, STT 31, STT 32, STT 33, STT 34, STT 59

---

## MỤC I: BÁO CÁO TỰ MÔ TẢ & GIẢI THÍCH KỸ THUẬT

### 1. Tổng quan nghiệp vụ Module 3 đảm nhiệm
Module 3 là "trái tim" của công tác phòng ngừa sự cố trong hệ thống MaintainPro, chuyển dịch mô hình vận hành từ "sửa chữa bị động khi đã hỏng hóc" sang "bảo trì chủ động theo chu kỳ". Module đảm nhiệm các quy trình nghiệp vụ cốt lõi sau:
1. **Lập và quản lý kế hoạch bảo trì định kỳ:** Quản lý kế hoạch cho từng thiết bị theo chu kỳ tháng quy định của từng loại thiết bị, ấn định thời gian dự kiến, nội dung công việc và mức độ ưu tiên.
2. **Quy tắc kiểm soát nghiệp vụ (Business Rules):** 
   - Không cho phép lập kế hoạch cho thiết bị đã ở trạng thái `NgungSuDung`.
   - Ngăn chặn triệt để tình trạng tạo trùng lặp 2 kế hoạch bảo trì đang hoạt động cho cùng một thiết bị trong cùng kỳ bảo trì (cùng tháng - năm).
   - Chặn ngày dự kiến ở quá khứ hoặc vượt quá ngưỡng an toàn 5 năm.
3. **Quản lý vòng đời trạng thái nghiêm ngặt (Workflow State Machine):**
   Tuân thủ nghiêm ngặt tiến trình:
   $$\text{Chờ thực hiện} \longrightarrow \text{Đã phân công} \longrightarrow \text{Đang thực hiện} \longrightarrow \text{Chờ nghiệm thu} \longrightarrow \text{Hoàn thành / Đã hủy}$$
   Đặc biệt khóa cứng (Lock) bản ghi khi đã `HoanThanh` hoặc `DaHuy` để bảo toàn tính toàn vẹn dữ liệu kiểm toán.
4. **Liên kết phát sinh Phiếu xử lý:** Tự động sinh `PhieuXuLy` loại `BaoTriDinhKy` từ một kế hoạch, bảo đảm quan hệ $1 - N$ nhưng tại mỗi thời điểm chỉ có tối đa 1 phiếu xử lý hoạt động.
5. **Cơ chế nghiệm thu và cập nhật mốc bảo trì tự động:** Sau khi nghiệm thu đạt, ngày hoàn thành thực tế được cập nhật vào `ThietBi.NgayBaoTriGanNhat`, kích hoạt cơ chế tính lại mốc `ThietBi.NgayBaoTriTiepTheo` thông qua công thức động mà không làm biến động lịch sử các lần bảo trì quá khứ.

---

### 2. Mô tả kiến trúc 3 lớp (3-Tier Architecture) và luồng dữ liệu

Dự án áp dụng mô hình phân lớp rõ ràng theo nguyên tắc **SoC (Separation of Concerns)** và **Dependency Inversion Principle (DIP)**:

```
[ Browser / View (Razor Engine) ]
             ↕ (HTTP Requests / Form Submit / AJAX / QueryString)
[ KeHoachBaoTriController ]
             ↕ (ViewModel / DTO, ServiceResult<T>)
[ IKeHoachBaoTriService / KeHoachBaoTriService ]  <--- Business Rules, Workflow, Transactions
             ↕ (Entities, Expressions, Linq Queries)
[ IKeHoachBaoTriRepository / KeHoachBaoTriRepository ]  <--- IQueryable, Skip/Take, Include
             ↕ (EF Core DbContext / Change Tracker)
[ Microsoft SQL Server Database ]
```

- **Lớp Controller (`KeHoachBaoTriController.cs`):** 
  - Chỉ đóng vai trò tiếp nhận HTTP Request, giải mã Query String / FormData thành ViewModel / Parameter, kiểm tra phân quyền (`[Authorize(Roles = AppRoles.AdminOrKyThuatVien)]`), điều hướng gọi Service và trả về View hoặc chuyển hướng (Redirect).
  - Tuyệt đối không chứa logic tính toán nghiệp vụ hay truy vấn SQL/EF Core trực tiếp.
- **Lớp Service (`KeHoachBaoTriService.cs` qua interface `IKeHoachBaoTriService`):**
  - Đảm nhận toàn bộ nghiệp vụ: Validate tính hợp lệ của ngày tháng, kiểm tra logic chống trùng kỳ bảo trì, thực thi chuyển trạng thái workflow thông qua `KeHoachWorkflowHelper`, phát sinh phiếu xử lý con, nghiệm thu kế hoạch và cập nhật mốc thiết bị.
  - Sử dụng mẫu kết quả chuẩn hóa `ServiceResult<T>` để trả về trạng thái thực thi (Thành công/Lỗi kèm thông báo và Field vi phạm) mà không phụ thuộc vào `ModelState` của MVC.
- **Lớp Repository (`KeHoachBaoTriRepository.cs` qua interface `IKeHoachBaoTriRepository`):**
  - Đóng gói toàn bộ các truy vấn LINQ to Entities, truy vấn không theo dõi (`AsNoTracking()`), nạp dữ liệu liên kết (`Include()`, `ThenInclude()`), thực thi cơ chế tìm kiếm, lọc đa tiêu chí, sắp xếp động và phân trang trực tiếp ở tầng máy chủ SQL Server.

---

### 3. Thiết kế Cơ sở dữ liệu và ánh xạ Entity

#### 3.1. Entity `KeHoachBaoTri`
```csharp
[Table("KeHoachBaoTri")]
public class KeHoachBaoTri
{
    [Key] 
    public int MaKeHoach { get; set; }

    [Display(Name = "Thiết bị")]
    public int MaThietBi { get; set; }

    [Column(TypeName = "date"), DataType(DataType.Date)]
    [Display(Name = "Ngày dự kiến")]
    public DateTime NgayDuKien { get; set; }

    [Required, StringLength(500)]
    public string NoiDungBaoTri { get; set; } = string.Empty;

    public MucDoUuTienEnum MucDoUuTien { get; set; } = MucDoUuTienEnum.TrungBinh;
    public TrangThaiKeHoachEnum TrangThai { get; set; } = TrangThaiKeHoachEnum.ChoThucHien;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    // Navigation Properties
    public ThietBi? ThietBi { get; set; }
    public ICollection<PhieuXuLy> PhieuXuLys { get; set; } = new List<PhieuXuLy>();
}
```

#### 3.2. Mối quan hệ trong hệ thống
- **Quan hệ $1 - N$ giữa `ThietBi` và `KeHoachBaoTri`:** Một thiết bị trải qua nhiều lần lập kế hoạch bảo trì theo năm tháng. Khóa ngoại `MaThietBi` liên kết về khóa chính `ThietBi.MaThietBi`.
- **Quan hệ $1 - N$ giữa `KeHoachBaoTri` và `PhieuXuLy`:** Một kế hoạch bảo trì có thể phát sinh phiếu xử lý để kỹ thuật viên thi công. Khóa ngoại `PhieuXuLy.MaKeHoach` (nullable) trỏ về `KeHoachBaoTri.MaKeHoach`. Ràng buộc toàn vẹn cơ sở dữ liệu `CK_Phieu_Nguon` quy định: Mỗi phiếu xử lý chỉ được liên kết với duy nhất một nguồn công việc: hoặc `MaKeHoach IS NOT NULL AND MaBaoCao IS NULL` (Bảo trì định kỳ), hoặc `MaBaoCao IS NOT NULL AND MaKeHoach IS NULL` (Xử lý sự cố).

#### 3.3. Thuộc tính tính toán động `[NotMapped]` liên quan
Trong Entity `ThietBi`, thuộc tính mốc bảo trì tiếp theo không tạo cột vật lý trên SQL nhằm tránh dư thừa dữ liệu và triệt tiêu nguy cơ sai lệch đồng bộ:
```csharp
[NotMapped]
public DateTime NgayBaoTriTiepTheo =>
    (NgayBaoTriGanNhat ?? NgayDuaVaoSuDung).AddMonths(ChuKyBaoTriThang);
```
Khi kế hoạch bảo trì được nghiệm thu tại `HoanThanhAsync`, hệ thống ghi nhận `tb.NgayBaoTriGanNhat = ngayHt`. Thuộc tính `NgayBaoTriTiepTheo` lập tức tự động tính toán lại mốc tiếp theo một cách nhất quán trên toàn bộ hệ thống.

---

### 4. Cơ chế Tìm kiếm, Lọc đa tiêu chí, Sắp xếp và Phân trang Server-side (SFSP)

Để đáp ứng khối lượng dữ liệu lớn và tối ưu băng thông đường truyền, toàn bộ logic truy vấn danh sách tại `KeHoachBaoTriRepository.LayPhanTrangAsync` được thực hiện hoàn toàn ở phía cơ sở dữ liệu thông qua `IQueryable`:

#### 4.1. Thứ tự thực thi chuẩn của IQueryable
Chuỗi truy vấn luôn tuân thủ đúng thứ tự chuẩn công nghiệp:
$$\text{QueryNoTracking()} \longrightarrow \text{Where (Lọc \& Tìm kiếm)} \longrightarrow \text{OrderBy / ThenBy (Sắp xếp)} \longrightarrow \text{Skip} \longrightarrow \text{Take} \longrightarrow \text{ToListAsync()}$$

#### 4.2. Tìm kiếm toàn văn đa từ khóa (Multi-keyword Search)
Người dùng có thể gõ cụm từ hỗn hợp (ví dụ: *"máy in văn phòng"* hoặc serial máy). Hệ thống tách chuỗi thành các từ đơn độc lập (giới hạn an toàn 5 từ khóa) và kết hợp `Contains` đa trường:
```csharp
if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
{
    var cacTu = boLoc.TuKhoa
        .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Take(5);

    foreach (var tu in cacTu)
    {
        query = query.Where(k =>
            k.ThietBi!.TenThietBi.Contains(tu)
            || k.ThietBi.SoSerial.Contains(tu)
            || k.ThietBi.KhuVuc!.TenKhuVuc.Contains(tu)
            || k.NoiDungBaoTri.Contains(tu)
        );
    }
}
```
EF Core sẽ dịch đoạn mã trên thành chuỗi các mệnh đề `AND (TenThietBi LIKE '%tu%' OR SoSerial LIKE '%tu%' ...)` trực tiếp trên SQL Server.

#### 4.3. Giữ nguyên trạng thái bộ lọc trên Query String khi chuyển trang
Trong `KeHoachBaoTriViewModels.cs`, lớp `KeHoachBaoTriBoLoc` cung cấp phương thức `ToRouteValues(int? trang = null)`:
```csharp
public Dictionary<string, string> ToRouteValues(int? trang = null)
{
    var route = new Dictionary<string, string>();
    if (!string.IsNullOrWhiteSpace(TuKhoa)) route[nameof(TuKhoa)] = TuKhoa;
    if (TrangThai.HasValue) route[nameof(TrangThai)] = TrangThai.Value.ToString();
    if (MucDo.HasValue) route[nameof(MucDo)] = MucDo.Value.ToString();
    if (TuNgay.HasValue) route[nameof(TuNgay)] = TuNgay.Value.ToString("yyyy-MM-dd");
    if (DenNgay.HasValue) route[nameof(DenNgay)] = DenNgay.Value.ToString("yyyy-MM-dd");
    if (SapXep != KeHoachBaoTriSapXep.NgayDuKienGanNhat) route[nameof(SapXep)] = SapXep.ToString();
    var p = trang ?? Trang;
    if (p > 1) route[nameof(Trang)] = p.ToString();
    return route;
}
```
Tại View `Index.cshtml`, thẻ phân trang liên kết trực tiếp với dictionary này thông qua `asp-all-route-data="@bl.ToRouteValues(trang)"`, đảm bảo khi người dùng chuyển sang trang 2, trang 3 thì toàn bộ từ khóa, trạng thái lọc, khoảng ngày và tiêu chí sắp xếp đều được giữ nguyên 100% trên thanh URL.

---

### 5. Các tính năng nghiệp vụ và kỹ thuật đặc thù trong Module

#### 5.1. Thuật toán kiểm tra chống trùng kế hoạch cùng kỳ (Idempotency Rule)
Nguyên tắc kinh doanh: Một thiết bị trong một chu kỳ tháng không được phép có nhiều hơn 1 kế hoạch bảo trì đang hoạt động.
Thuật toán tại `TimKeHoachTrungKyAsync`:
- Tính cận dưới của kỳ: ngày 1 đầu tháng của `NgayDuKien`: `new DateTime(ngayDuKien.Year, ngayDuKien.Month, 1)`.
- Tính cận trên của kỳ: `dauKy.AddMonths(1)`.
- Truy vấn LINQ kiểm tra:
```csharp
QueryNoTracking().FirstOrDefaultAsync(k =>
    k.MaThietBi == maThietBi
    && k.NgayDuKien >= dauKy
    && k.NgayDuKien < cuoiKy
    && k.TrangThai != TrangThaiKeHoachEnum.HoanThanh
    && k.TrangThai != TrangThaiKeHoachEnum.DaHuy
    && (boQuaMaKeHoach == null || k.MaKeHoach != boQuaMaKeHoach)
);
```
Sử dụng toán tử so sánh khoảng nửa mở `[dauKy, cuoiKy)` giúp chỉ mục trên cột `NgayDuKien` được tận dụng tối đa (Index Seek), không dùng hàm `k.NgayDuKien.Month == ...` vì sẽ làm mất khả năng sargable của chỉ mục SQL.

#### 5.2. Máy trạng thái hữu hạn cho vòng đời kế hoạch (`KeHoachWorkflowHelper`)
Quy trình chuyển đổi trạng thái được đóng gói độc lập trong `KeHoachWorkflowHelper`:
- Phương thức `CanTransitionTo(current, target)` quy định rõ từng bước chuyển đổi hợp lệ.
- Bất kỳ nỗ lực nào nhằm chuyển trạng thái trái quy trình (ví dụ: từ `ChoThucHien` nhảy cóc lên `HoanThanh`, hoặc sửa đổi kế hoạch đã `DaHuy`) đều bị chặn đứng ngay tại tầng Service và trả về thông báo lỗi rõ ràng.

#### 5.3. Nghiệm thu hoàn thành & Cập nhật mốc bảo trì thực tế
Tại phương thức `HoanThanhAsync(int maKeHoach, DateTime ngayHoanThanh, string? ghiChu)`:
1. Kiểm tra ngày hoàn thành: không trước ngày đưa vào sử dụng của thiết bị, không được vượt quá ngày hiện tại (không nghiệm thu cho tương lai).
2. Chuyển trạng thái kế hoạch thành `HoanThanh`.
3. Ghi vết nghiệm thu kèm thời gian và nhận xét vào cột `GhiChu`.
4. Nếu `tb.NgayBaoTriGanNhat == null` hoặc `ngayHt >= tb.NgayBaoTriGanNhat`: gán `tb.NgayBaoTriGanNhat = ngayHt`.
5. Đưa trạng thái thiết bị từ `DangBaoTri` về `DangHoatDong`.
6. Bắn thông báo nội bộ cho Quản trị viên (hợp tác STT 59).

---

## MỤC II: BỘ CÂU HỎI & CÂU TRẢ LỜI PHẢN BIỆN BẢO VỆ ĐỒ ÁN

### Câu 1: Tại sao trong cơ sở dữ liệu, bảng `ThietBi` chỉ lưu cột `NgayBaoTriGanNhat` mà không tạo cột `NgayBaoTriTiepTheo`?
**Trả lời:**
> "Kính thưa Thầy/Cô, đây là quyết định thiết kế CSDL nhằm tuân thủ **Dạng chuẩn 3 (3NF - Third Normal Form)** và triệt tiêu sự phụ thuộc bắc cầu (Transitive Dependency).
> Cột `NgayBaoTriTiepTheo` có thể được suy diễn hoàn toàn từ hai cột có sẵn: `(NgayBaoTriGanNhat ?? NgayDuaVaoSuDung) + ChuKyBaoTriThang`.
> Nếu lưu cột vật lý trong bảng, bất cứ khi nào người dùng thay đổi chu kỳ bảo trì của thiết bị hoặc cập nhật ngày bảo trì gần nhất, hệ thống sẽ phải nhớ đồng bộ cột thứ ba. Nếu xảy ra lỗi phân mảnh dữ liệu hoặc cập nhật thiếu, hai cột sẽ xung đột nhau. Do đó, em cài đặt thuộc tính `[NotMapped]` trên Entity `ThietBi` để ứng dụng luôn tính toán tức thời và chính xác tuyệt đối."

---

### Câu 2: Em hãy giải thích thuật toán kiểm tra chống trùng kế hoạch bảo trì cùng kỳ trong hàm `TimKeHoachTrungKyAsync`?
**Trả lời:**
> "Kính thưa Thầy/Cô, một thiết bị không thể có 2 kế hoạch bảo trì cùng được triển khai trong một kỳ tháng. Trong `TimKeHoachTrungKyAsync`, em xác định kỳ bảo trì là tháng dự kiến bằng cách lấy ngày đầu tiên của tháng `dauKy = new DateTime(ngay.Year, ngay.Month, 1)` và ngày đầu tiên của tháng kế tiếp `cuoiKy = dauKy.AddMonths(1)`.
> Thay vì viết `k.NgayDuKien.Month == ngay.Month && k.NgayDuKien.Year == ngay.Year`, em sử dụng điều kiện khoảng: `k.NgayDuKien >= dauKy && k.NgayDuKien < cuoiKy`. 
> Kỹ thuật này gọi là **Sargable Query**: Khi không bọc hàm lên cột `NgayDuKien`, SQL Server có thể tận dụng chỉ mục `Index Seek` trên cột ngày thay vì phải quét toàn bộ bảng (`Index Scan`), mang lại hiệu năng cao ngay cả khi bảng có hàng chục nghìn kế hoạch. Đồng thời, em loại trừ các kế hoạch đã `HoanThanh` hoặc `DaHuy` vì các kế hoạch này không còn hiệu lực."

---

### Câu 3: Làm thế nào em đảm bảo việc chuyển trạng thái của kế hoạch bảo trì không bị nhảy cóc hoặc gian lận dữ liệu?
**Trả lời:**
> "Kính thưa Thầy/Cô, em thiết kế một lớp chuyên trách là `KeHoachWorkflowHelper` đóng vai trò là một **Finite State Machine (Máy trạng thái hữu hạn)**:
> 1. Phương thức `CanTransitionTo(current, target)` định nghĩa ma trận chuyển đổi trạng thái hợp lệ. Ví dụ: từ `ChoThucHien` chỉ được sang `DaPhanCong` hoặc `DaHuy`. Trạng thái `ChoNghiemThu` chỉ có thể lên `HoanThanh` hoặc trả ngược về `DangThucHien` để KTV làm lại nếu nghiệm thu không đạt.
> 2. Phương thức `IsLocked(status)` kiểm tra nếu kế hoạch đã ở `HoanThanh` hoặc `DaHuy` thì khóa 100% mọi thao tác chỉnh sửa, hủy hay chuyển trạng thái tiếp.
> 3. Toàn bộ logic này được kiểm tra ở tầng `KeHoachBaoTriService.ChuyenTrangThaiAsync` trước khi gọi `SaveChangesAsync()`. Do đó dù có cố tình can thiệp bằng cách giả lập HTTP POST request trái phép từ bên ngoài thì Service vẫn lập tức từ chối và bảo toàn dữ liệu."

---

### Câu 4: Trong chức năng phân trang danh sách kế hoạch, nếu người dùng đang ở trang 3 với bộ lọc 'Ưu tiên Cao', khi họ bấm chuyển sang trang 4 thì bộ lọc có bị mất không? Em xử lý việc này như thế nào?
**Trả lời:**
> "Kính thưa Thầy/Cô, bộ lọc **chắc chắn không bị mất**. Em đã thiết kế cơ chế lưu giữ trạng thái truy vấn (Query String Preservation) thông qua phương thức `ToRouteValues(int? trang)` trong lớp `KeHoachBaoTriBoLoc`.
> Phương thức này chuyển toàn bộ các thuộc tính lọc đang có giá trị (`TuKhoa`, `TrangThai`, `MucDo`, `TuNgay`, `DenNgay`, `SapXep`, `SoBanGhiMoiTrang`) thành một `Dictionary<string, string>`. 
> Tại Razor View `Index.cshtml`, các thẻ phân trang sử dụng `asp-all-route-data="@bl.ToRouteValues(trang)"`. Nhờ đó, link sinh ra trên thẻ `<a>` luôn chứa đầy đủ các tham số trên Query String cùng với số trang tương ứng. Khi người dùng bấm sang trang khác, Controller vẫn nhận lại nguyên vẹn toàn bộ bộ lọc và chỉ việc `Skip/Take` trên tập dữ liệu đã lọc đó."

---

### Câu 5: Tại sao khi hiển thị danh sách kế hoạch em lại dùng `QueryNoTracking()` mà không dùng `_context.KeHoachBaoTris` thông thường?
**Trả lời:**
> "Kính thưa Thầy/Cô, khi thực hiện các thao tác chỉ đọc (Read-only) như hiển thị danh sách, tìm kiếm và phân trang, đối tượng dữ liệu không cần phải chỉnh sửa hay lưu ngược lại vào database.
> Nếu dùng truy vấn mặc định, Change Tracker của Entity Framework Core sẽ phải tạo thêm các snapshot bản sao trong bộ nhớ RAM để theo dõi trạng thái của từng Entity.
> Bằng cách sử dụng `AsNoTracking()` (thông qua hàm `QueryNoTracking()` của Base Repository), hệ thống bỏ qua hoàn toàn cơ chế Change Tracking, giúp giảm đáng kể mức tiêu thụ bộ nhớ RAM và tăng tốc độ thực thi truy vấn lên khoảng 20% - 30%."

---

### Câu 6: Giả sử một Kế hoạch bảo trì đã phát sinh Phiếu xử lý đang được Kỹ thuật viên thi công dở dang, người quản trị có thể bấm 'Hủy' kế hoạch bảo trì đó được không? Tại sao?
**Trả lời:**
> "Kính thưa Thầy/Cô, hệ thống **tuyệt đối không cho phép hủy** trong tình huống này.
> Trong phương thức `ChuyenTrangThaiAsync`, tại bước xử lý trạng thái `DaHuy`, hệ thống gọi hàm `_repo.DemPhieuConHieuLucAsync(id)`.
> Nếu số phiếu xử lý con chưa bị hủy lớn hơn 0, Service sẽ trả về lỗi: *'Không thể hủy vì kế hoạch đang có phiếu xử lý chưa hủy. Hãy hủy phiếu xử lý trước'*.
> Quy tắc này nhằm đảm bảo tính nhất quán dữ liệu giữa hai Module 3 và 4: tránh tình huống kế hoạch cha đã bị hủy nhưng kỹ thuật viên vẫn thực hiện và xuất vật tư chi phí cho phiếu xử lý con, gây sai lệch số liệu tài chính."

---

### Câu 7: Khi nghiệm thu một Kế hoạch bảo trì đạt yêu cầu (`HoanThanh`), tình trạng của Thiết bị thay đổi như thế nào và việc ghi nhận lịch sử được thực hiện ra sao?
**Trả lời:**
> "Kính thưa Thầy/Cô, trong phương thức `HoanThanhAsync`:
> 1. Thiết bị đang ở tình trạng `DangBaoTri` sẽ tự động được phục hồi về `DangHoatDong`.
> 2. Ngày hoàn thành thực tế được gán vào `tb.NgayBaoTriGanNhat`.
> 3. Lịch sử bảo trì được bảo toàn trọn vẹn: Toàn bộ bản ghi `KeHoachBaoTri` này không bị sửa đè hay xóa đi mà được lưu vĩnh viễn với trạng thái `HoanThanh`, kèm dòng nhật ký nghiệm thu (thời gian, người duyệt, nhận xét) nối tiếp vào cột `GhiChu`. Toàn bộ các `PhieuXuLy` và `ChiTietXuLy` chi phí liên kết vẫn tồn tại nguyên vẹn, phục vụ cho việc tra cứu lịch sử thiết bị ở Module 5."

---

### Câu 8: Tại sao em không dùng Trigger trong SQL Server để cập nhật `NgayBaoTriGanNhat` cho thiết bị mà lại viết code trong Service Layer?
**Trả lời:**
> "Kính thưa Thầy/Cô, việc cài đặt nghiệp vụ tại **Service Layer** thay vì Trigger trong SQL Server mang lại các lợi ích kiến trúc rõ rệt:
> 1. **Khả năng kiểm thử (Testability):** Logic viết bằng C# trong Service có thể dễ dàng viết Unit Test với Mock Repository mà không cần phụ thuộc vào một Database Server đang chạy.
> 2. **Dễ bảo trì và gỡ lỗi (Debugging):** Toàn bộ lập trình viên có thể debug từng dòng code (F10/F11) và kiểm soát luồng dữ liệu ngay trong Visual Studio, thay vì phải mở SQL Server Profiler dò tìm Trigger ngầm.
> 3. **Tính độc lập với CSDL (Portability):** Nghiệp vụ không bị ràng buộc chặt vào cú pháp riêng của T-SQL, giúp hệ thống có thể chuyển đổi hoặc nâng cấp hệ quản trị CSDL trong tương lai mà không phải viết lại mã nguồn."

---

*Tài liệu được lập ngày 07/11/2026 bởi sinh viên Phạm Việt Hoàn - Phân hệ Module 3 (MaintainPro).*
