using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services.Products;
using Backend.DTOs.Products.Queries;
using Backend.DTOs.Products.Requests;
using Backend.DTOs.Common.Responses;
using Backend.Mappings;

namespace Backend.Controllers;

/// <summary>
/// Products API Controller - handles all product CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get products (cursor-based pagination + filters)
    /// </summary>
    /// <returns>Paged list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Product>>> GetAll([FromQuery] ProductQueryParameters query)
    {
        var products = await _productService.GetAllAsync(query);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product if found</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="dto">Product data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = dto.ToEntity();

        var createdProduct = await _productService.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Updated product data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
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

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}


