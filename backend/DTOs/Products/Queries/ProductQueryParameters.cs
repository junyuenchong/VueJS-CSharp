namespace Backend.DTOs.Products.Queries;

/// <summary>
/// Query parameters for cursor-based pagination and filtering.
/// </summary>
public class ProductQueryParameters
{
    /// <summary>
    /// Cursor for keyset pagination. Returns results with Id greater than this value.
    /// </summary>
    public int? Cursor { get; set; }

    /// <summary>
    /// Page size (max items to return).
    /// </summary>
    public int Limit { get; set; } = 20;

    /// <summary>
    /// Free-text search applied to Name/Description.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Minimum price (inclusive).
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maximum price (inclusive).
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// If true, only products with Stock > 0 are returned.
    /// </summary>
    public bool? InStock { get; set; }
}

