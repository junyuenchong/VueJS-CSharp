using Backend.DTOs.Products.Requests;
using Backend.Mappings;

namespace Backend.UnitTests;

public class ProductMappingsTests
{
    [Fact]
    public void CreateProductDto_ToEntity_maps_fields()
    {
        var dto = new CreateProductDto
        {
            Name = "Laptop",
            Description = "14\"",
            Price = 999.99m,
            Stock = 10
        };

        var entity = dto.ToEntity();

        Assert.Equal("Laptop", entity.Name);
        Assert.Equal("14\"", entity.Description);
        Assert.Equal(999.99m, entity.Price);
        Assert.Equal(10, entity.Stock);
    }

    [Fact]
    public void UpdateProductDto_ToEntity_maps_fields_including_id()
    {
        var dto = new UpdateProductDto
        {
            Id = 7,
            Name = "Mouse",
            Description = "Wireless",
            Price = 25m,
            Stock = 200
        };

        var entity = dto.ToEntity();

        Assert.Equal(7, entity.Id);
        Assert.Equal("Mouse", entity.Name);
        Assert.Equal("Wireless", entity.Description);
        Assert.Equal(25m, entity.Price);
        Assert.Equal(200, entity.Stock);
    }
}

