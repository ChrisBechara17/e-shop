using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EShopOnWeb.Models;

/// <summary>
/// Represents a product grouping for browsing and filtering.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
