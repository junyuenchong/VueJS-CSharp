using Backend.Models;
using Backend.DTOs.Products.Queries;
using Backend.DTOs.Common.Pagination;

namespace Backend.Services.Products;

/*
 * Product business logic for CRUD + paged listing.
 */
public interface IProductService
{
    /*
     * List products with cursor/keyset pagination and optional filters/search.
     */
    Task<PagedResult<Product>> GetAllAsync(ProductQueryParameters query);

    /* Get one product by id. */
    Task<Product?> GetByIdAsync(int id);

    /* Create a new product row. */
    Task<Product> CreateAsync(Product product);

    /* Update a product row; returns false when it doesn't exist. */
    Task<bool> UpdateAsync(int id, Product product);

    /* Delete by id; returns false when it doesn't exist. */
    Task<bool> DeleteAsync(int id);

    /* Exists check used by update logic. */
    Task<bool> ExistsAsync(int id);
}
