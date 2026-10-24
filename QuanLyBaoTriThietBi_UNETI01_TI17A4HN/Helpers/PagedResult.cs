#nullable enable
using Microsoft.EntityFrameworkCore;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

public class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    /// Chuyển đổi kiểu dữ liệu các phần tử trong trang nhưng giữ nguyên thông tin phân trang
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector) =>
        new()
        {
            Items = Items.Select(selector).ToList(),
            Page = Page,
            PageSize = PageSize,
            TotalItems = TotalItems,
        };

    public static async Task<PagedResult<T>> CreateAsync(
        IQueryable<T> source,
        int page,
        int pageSize
    )
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 10;

        var total = await source.CountAsync();

        // Tránh tình trạng trang vượt quá tổng số trang khi dữ liệu bị xóa bớt
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        if (page > totalPages && totalPages > 0)
            page = totalPages;

        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }
}
