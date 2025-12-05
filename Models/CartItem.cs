using System.ComponentModel.DataAnnotations;

namespace EShopOnWeb.Models;

/// <summary>
/// Item stored in an anonymous session cart.
/// </summary>
public class CartItem
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public string SessionId { get; set; } = string.Empty;
}
