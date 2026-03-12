using Backend.DTOs.Products.Requests;
using Backend.Models;

namespace Backend.Mappings;

/// <summary>
/// Centralized mappings to avoid duplicating DTO-to-Entity conversion in controllers/services.
/// </summary>
public static class ProductMappings
{
    public static Product ToEntity(this CreateProductDto dto)
    {
        return new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock
        };
    }

    public static Product ToEntity(this UpdateProductDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock
        };
    }
}

