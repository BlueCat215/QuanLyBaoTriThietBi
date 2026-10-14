// Họ tên: <điền họ tên người tạo khung>
// MSSV: <điền MSSV>
// Nội dung: Chung - Cài đặt Repository tổng quát bằng EF Core (khung dùng chung cả nhóm)
#nullable enable
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual IQueryable<T> Query() => _dbSet;

    public virtual IQueryable<T> QueryNoTracking() => _dbSet.AsNoTracking();

    public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public virtual async Task<List<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();

    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.FirstOrDefaultAsync(predicate);

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate is null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

    public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Remove(T entity) => _dbSet.Remove(entity);

    public virtual Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
