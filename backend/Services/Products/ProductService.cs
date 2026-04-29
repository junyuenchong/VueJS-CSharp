using Backend.Data;
using Backend.Models;
using Backend.DTOs.Products.Queries;
using Backend.DTOs.Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Products;

/*
 * Handles product reads and writes using EF Core.
 * - Supports fast keyset pagination.
 * - Filters are optimized for database indexes.
 * - Uses FULLTEXT for text search when possible; falls back to LIKE for short terms.
 */
public class ProductService : IProductService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    /*
     * List products with filters/search, then keyset page by Id.
     * nextCursor is returned when there are more rows.
     */
    public async Task<PagedResult<Product>> GetAllAsync(ProductQueryParameters query)
    {
        var limit = query.Limit <= 0 ? DefaultPageSize : Math.Min(query.Limit, MaxPageSize);

        IQueryable<Product> q = _context.Products.AsNoTracking();

        // Filters (business-friendly)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            // Use MySQL FULLTEXT path for longer tokens; keep LIKE fallback for short terms.
            if (term.Length >= 3)
            {
                q = q.Where(p => EF.Functions.Match(
                    new[] { p.Name, p.Description! },
                    term,
                    MySqlMatchSearchMode.NaturalLanguage) > 0);
            }
            else
            {
                q = q.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)));
            }
        }

        if (query.MinPrice.HasValue)
            q = q.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            q = q.Where(p => p.Price <= query.MaxPrice.Value);

        if (query.InStock.HasValue && query.InStock.Value)
            q = q.Where(p => p.Stock > 0);

        // Cursor-based (keyset) pagination by PK (Id)
        if (query.Cursor.HasValue)
            q = q.Where(p => p.Id > query.Cursor.Value);

        // Always order by cursor column
        var items = await q
            .OrderBy(p => p.Id)
            .Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock
            })
            .Take(limit + 1) // fetch one extra to determine next cursor
            .ToListAsync();

        var hasMore = items.Count > limit;
        int? nextCursor = null;
        if (hasMore)
        {
            var last = items[limit - 1];
            nextCursor = last.Id;
            items.RemoveAt(limit);
        }

        return new PagedResult<Product>
        {
            Items = items,
            NextCursor = nextCursor,
            Limit = limit
        };
    }

    /* Read one product by primary key. */
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock
            })
            .FirstOrDefaultAsync();
    }

    /* Create a product row. */
    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /*
     * Update a product row.
     * Returns false when the id doesn't exist.
     */
    public async Task<bool> UpdateAsync(int id, Product product)
    {
        if (id != product.Id)
            return false;

        // EF Core bulk operations aren't supported by the InMemory provider (commonly used in tests).
        // Fall back to tracked entity updates so E2E tests behave like a real relational provider.
        if (string.Equals(_context.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null)
                return false;

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;

            await _context.SaveChangesAsync();
            return true;
        }

        var affected = await _context.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Name, product.Name)
                .SetProperty(p => p.Description, product.Description)
                .SetProperty(p => p.Price, product.Price)
                .SetProperty(p => p.Stock, product.Stock));
        return affected > 0;
    }

    /*
     * Delete a product row.
     * Returns false when the id doesn't exist.
     */
    public async Task<bool> DeleteAsync(int id)
    {
        // EF Core bulk operations aren't supported by the InMemory provider (commonly used in tests).
        if (string.Equals(_context.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null)
                return false;

            _context.Products.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        var affected = await _context.Products
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();
        return affected > 0;
    }

    /* Fast exists check for update logic. */
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }
}
