using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Products.Requests;

public class CreateProductDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0")]
    public int Stock { get; set; }
}

