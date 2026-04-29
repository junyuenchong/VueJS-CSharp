using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services.Products;
using Backend.DTOs.Products.Queries;
using Backend.DTOs.Products.Requests;
using Backend.DTOs.Common.Pagination;
using Backend.Mappings;

namespace Backend.Controllers;

/*
 * Products endpoints: CRUD + cursor pagination + filters.
 * Keep read paths server-side filtered/ordered to reduce payload.
 */
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /*
     * List products (cursor-based pagination + filters).
     */
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Product>>> GetAll([FromQuery] ProductQueryParameters query)
    {
        try
        {
            var products = await _productService.GetAllAsync(query);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list products.");
            return Problem("An unexpected error occurred while listing products.");
        }
    }

    /* Get one product by id. */
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get product {ProductId}.", id);
            return Problem("An unexpected error occurred while getting the product.");
        }
    }

    /* Create a new product. */
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = dto.ToEntity();

            var createdProduct = await _productService.CreateAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product.");
            return Problem("An unexpected error occurred while creating the product.");
        }
    }

    /* Update an existing product. Returns 204 on success. */
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = dto.ToEntity();

            var updated = await _productService.UpdateAsync(id, product);
            if (!updated)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product {ProductId}.", id);
            return Problem("An unexpected error occurred while updating the product.");
        }
    }

    /* Delete a product by id. Returns 204 on success. */
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _productService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId}.", id);
            return Problem("An unexpected error occurred while deleting the product.");
        }
    }
}


