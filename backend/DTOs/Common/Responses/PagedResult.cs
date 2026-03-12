namespace Backend.DTOs.Common.Responses;

/// <summary>
/// Shared response wrapper for cursor-based pagination.
/// Keep this in Common so other modules (Users, Orders, etc.) reuse it.
/// </summary>
public class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Cursor to fetch the next page. Null means there is no next page.
    /// </summary>
    public int? NextCursor { get; init; }

    public int Limit { get; init; }
}

