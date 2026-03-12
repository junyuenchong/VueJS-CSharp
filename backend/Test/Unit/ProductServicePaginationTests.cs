using Backend.Data;
using Backend.DTOs.Products.Queries;
using Backend.Models;
using Backend.Services.Products;
using Microsoft.EntityFrameworkCore;

namespace Backend.UnitTests;

public class ProductServicePaginationTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_returns_first_page_and_next_cursor()
    {
        await using var db = CreateDbContext($"Unit-{Guid.NewGuid()}");
        db.Products.AddRange(
            new Product { Id = 1, Name = "A", Price = 1, Stock = 1 },
            new Product { Id = 2, Name = "B", Price = 1, Stock = 1 },
            new Product { Id = 3, Name = "C", Price = 1, Stock = 1 }
        );
        await db.SaveChangesAsync();

        var service = new ProductService(db);
        var result = await service.GetAllAsync(new ProductQueryParameters { Limit = 2 });

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(new[] { 1, 2 }, result.Items.Select(p => p.Id).ToArray());
        Assert.Equal(2, result.NextCursor);
    }

    [Fact]
    public async Task GetAllAsync_with_cursor_returns_next_page()
    {
        await using var db = CreateDbContext($"Unit-{Guid.NewGuid()}");
        db.Products.AddRange(
            new Product { Id = 1, Name = "A", Price = 1, Stock = 1 },
            new Product { Id = 2, Name = "B", Price = 1, Stock = 1 },
            new Product { Id = 3, Name = "C", Price = 1, Stock = 1 }
        );
        await db.SaveChangesAsync();

        var service = new ProductService(db);
        var result = await service.GetAllAsync(new ProductQueryParameters { Cursor = 2, Limit = 10 });

        Assert.Single(result.Items);
        Assert.Equal(3, result.Items[0].Id);
        Assert.Null(result.NextCursor);
    }
}

