using EShopOnWeb.Models;

namespace EShopOnWeb.Services;

public interface ICartService
{
    Task<List<CartItem>> GetCartItemsAsync(string sessionId);
    Task AddToCartAsync(string sessionId, int productId, int quantity);
    Task RemoveFromCartAsync(string sessionId, int cartItemId);
    Task ClearCartAsync(string sessionId);
}
