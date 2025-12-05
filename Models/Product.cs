using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopOnWeb.Models;

/// <summary>
/// Product available for purchase.
/// </summary>
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
