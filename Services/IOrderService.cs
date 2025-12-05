using EShopOnWeb.Models;

namespace EShopOnWeb.Services;

public interface IOrderService
{
    Task<Order> CreateOrderFromCartAsync(string sessionId, string customerName, string customerEmail);
    Task<Order?> GetOrderByIdAsync(int id);
}
