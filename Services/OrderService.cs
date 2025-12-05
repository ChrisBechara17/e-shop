using System;
using EShopOnWeb.Data;
using EShopOnWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopOnWeb.Services;

/// <summary>
/// Creates orders from cart contents and loads order data.
/// </summary>
public class OrderService : IOrderService
{
    private readonly ShopDbContext _context;

    public OrderService(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderFromCartAsync(string sessionId, string customerName, string customerEmail)
    {
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.SessionId == sessionId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        var order = new Order
        {
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in cartItems)
        {
            var unitPrice = item.Product?.Price ?? 0;
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice
            });
            order.TotalAmount += unitPrice * item.Quantity;
        }

        await _context.Orders.AddAsync(order);
        _context.CartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();
        return order;
    }

    public Task<Order?> GetOrderByIdAsync(int id)
    {
        return _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
