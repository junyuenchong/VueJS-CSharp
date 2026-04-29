namespace Backend.DTOs.Common.Pagination;

/*
 * Cursor-based page payload.
 */
public class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    /* Cursor for the next page. Null means no next page. */
    public int? NextCursor { get; init; }

    public int Limit { get; init; }
}
