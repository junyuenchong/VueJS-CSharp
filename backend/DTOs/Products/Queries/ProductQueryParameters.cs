namespace Backend.DTOs.Products.Queries;

/*
 * Query parameters for cursor pagination + filtering.
 */
public class ProductQueryParameters
{
    /*
     * Cursor for keyset pagination. Returns rows with Id > cursor.
     */
    public int? Cursor { get; set; }

    /*
     * Page size (max items to return).
     */
    public int Limit { get; set; } = 20;

    /*
     * Search term applied to Name/Description.
     */
    public string? Search { get; set; }

    /*
     * Minimum price (inclusive).
     */
    public decimal? MinPrice { get; set; }

    /*
     * Maximum price (inclusive).
     */
    public decimal? MaxPrice { get; set; }

    /*
     * If true, only products with Stock > 0 are returned.
     */
    public bool? InStock { get; set; }
}

