using EShopOnWeb.Models;

namespace EShopOnWeb.ViewModels;

public class CartViewModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity);
}
