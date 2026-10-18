// Họ tên: <điền họ tên người tạo khung>
// MSSV: <điền MSSV>
// Nội dung: Chung - Interface Repository tổng quát cho mọi Entity (khung dùng chung cả nhóm)
#nullable enable
using System.Linq.Expressions;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

/// <summary>
/// Repository tổng quát: chỉ truy cập dữ liệu (LINQ/EF Core), KHÔNG chứa luật nghiệp vụ.
/// Add/Update/Remove chỉ đánh dấu thay đổi; Service gọi SaveChangesAsync() để lưu một lần.
/// Mọi Repository dùng chung 1 AppDbContext (Scoped) nên SaveChangesAsync của repo nào cũng lưu toàn bộ thay đổi trong request.
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>Truy vấn có theo dõi (tracking) - dùng khi sắp Update/Remove.</summary>
    IQueryable<T> Query();

    /// <summary>Truy vấn chỉ đọc (AsNoTracking) - dùng cho danh sách, tìm kiếm, lọc, phân trang.</summary>
    IQueryable<T> QueryNoTracking();

    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);

    Task<int> SaveChangesAsync();
}
