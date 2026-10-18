// Họ tên: <điền họ tên người tạo khung>
// MSSV: <điền MSSV>
// Nội dung: Chung - Kết quả trả về của Service (thành công/thất bại + danh sách lỗi theo trường)
#nullable enable
namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

/// <summary>Một lỗi nghiệp vụ. Field = tên property trên form (rỗng "" nếu là lỗi chung của cả form).</summary>
public record ServiceError(string Field, string Message);

/// <summary>
/// Service KHÔNG ném exception cho lỗi nghiệp vụ thông thường mà trả ServiceResult;
/// Controller đưa lỗi vào ModelState bằng ModelState.AddServiceErrors(result).
/// </summary>
public class ServiceResult
{
    public List<ServiceError> Errors { get; } = new();
    public bool Succeeded => Errors.Count == 0;
    public string ErrorMessage => string.Join("; ", Errors.Select(e => e.Message));

    public static ServiceResult Ok() => new();

    public static ServiceResult Fail(string message, string field = "")
    {
        var r = new ServiceResult();
        r.Errors.Add(new ServiceError(field, message));
        return r;
    }

    public ServiceResult AddError(string message, string field = "")
    {
        Errors.Add(new ServiceError(field, message));
        return this;
    }
}

/// <summary>Kết quả có kèm dữ liệu (vd: tài khoản đăng nhập thành công).</summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data) => new() { Data = data };

    public static new ServiceResult<T> Fail(string message, string field = "")
    {
        var r = new ServiceResult<T>();
        r.Errors.Add(new ServiceError(field, message));
        return r;
    }
}
