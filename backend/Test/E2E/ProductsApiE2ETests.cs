using Backend.DTOs.Products.Requests;
using Backend.Models;
using System.Net;
using System.Net.Http.Json;

namespace Backend.E2ETests;

public class ProductsApiE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiE2ETests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Full_crud_flow_works()
    {
        // CREATE
        var create = new CreateProductDto
        {
            Name = "Monitor",
            Description = "27-inch",
            Price = 199.99m,
            Stock = 5
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Products", create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);

        // READ
        var getResponse = await _client.GetAsync($"/api/Products/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(fetched);
        Assert.Equal("Monitor", fetched!.Name);

        // UPDATE
        var update = new UpdateProductDto
        {
            Id = created.Id,
            Name = "Monitor (Updated)",
            Description = "27-inch",
            Price = 179.99m,
            Stock = 10
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/Products/{created.Id}", update);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getAfterUpdate = await _client.GetFromJsonAsync<Product>($"/api/Products/{created.Id}");
        Assert.NotNull(getAfterUpdate);
        Assert.Equal("Monitor (Updated)", getAfterUpdate!.Name);
        Assert.Equal(10, getAfterUpdate.Stock);

        // DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/Products/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // READ after delete => 404
        var getAfterDelete = await _client.GetAsync($"/api/Products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }
}

