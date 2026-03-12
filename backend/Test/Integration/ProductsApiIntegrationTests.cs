using Backend.Data;
using Backend.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Backend.IntegrationTests;

public class ProductsApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProductsApiIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_products_returns_200_and_paged_response_shape()
    {
        // Arrange: seed in-memory DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Products.RemoveRange(db.Products);
            db.Products.Add(new Product { Id = 1, Name = "Keyboard", Price = 79.99m, Stock = 50 });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Products?limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<PagedProductsResponse>();
        Assert.NotNull(json);
        Assert.NotNull(json!.Items);
        Assert.Single(json.Items);
        Assert.Equal(10, json.Limit);
    }

    // Small local type so the test is easy to read (matches your API response)
    private sealed class PagedProductsResponse
    {
        public List<ProductDto> Items { get; set; } = new();
        public int? NextCursor { get; set; }
        public int Limit { get; set; }
    }

    private sealed class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}

