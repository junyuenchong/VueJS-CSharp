using Backend.Data;
using Backend.Models;
using Backend.DTOs.Products.Queries;
using Backend.DTOs.Common.Responses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Product>> GetAllAsync(ProductQueryParameters query)
    {
        var limit = query.Limit;
        if (limit <= 0) limit = 20;
        if (limit > 100) limit = 100; // protect DB

        IQueryable<Product> q = _context.Products.AsNoTracking();

        // Filters (business-friendly)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(p =>
                p.Name.Contains(term) ||
                (p.Description != null && p.Description.Contains(term)));
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
            .Take(limit + 1) // fetch one extra to determine next cursor
            .ToListAsync();

        int? nextCursor = null;
        if (items.Count > limit)
        {
            var last = items[limit - 1];
            nextCursor = last.Id;
            items = items.Take(limit).ToList();
        }

        return new PagedResult<Product>
        {
            Items = items,
            NextCursor = nextCursor,
            Limit = limit
        };
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await (from product in _context.Products
                      where product.Id == id
                      select product).FirstOrDefaultAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> UpdateAsync(int id, Product product)
    {
        if (id != product.Id)
            return false;

        var exists = await ExistsAsync(id);
        if (!exists)
            return false;

        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await GetByIdAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await (from product in _context.Products
                      where product.Id == id
                      select product).AnyAsync();
    }
}
