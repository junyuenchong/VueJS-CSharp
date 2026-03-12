using Backend.Models;
using Backend.DTOs.Products.Queries;
using Backend.DTOs.Common.Responses;

namespace Backend.Services.Products;

public interface IProductService
{
    Task<PagedResult<Product>> GetAllAsync(ProductQueryParameters query);
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
