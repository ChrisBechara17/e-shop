using System;
using EShopOnWeb.Data;
using EShopOnWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopOnWeb.Services;

/// <summary>
/// Manages session-based cart operations.
/// </summary>
public class CartService : ICartService
{
    private readonly ShopDbContext _context;

    public CartService(ShopDbContext context)
    {
        _context = context;
    }

    public Task<List<CartItem>> GetCartItemsAsync(string sessionId)
    {
        return _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task AddToCartAsync(string sessionId, int productId, int quantity)
    {
        quantity = Math.Max(1, quantity);

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.SessionId == sessionId && ci.ProductId == productId);

        if (cartItem == null)
        {
            cartItem = new CartItem
            {
                SessionId = sessionId,
                ProductId = productId,
                Quantity = quantity
            };

            await _context.CartItems.AddAsync(cartItem);
        }
        else
        {
            cartItem.Quantity += quantity;
            _context.CartItems.Update(cartItem);
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(string sessionId, int cartItemId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.SessionId == sessionId);

        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string sessionId)
    {
        var items = await _context.CartItems
            .Where(ci => ci.SessionId == sessionId)
            .ToListAsync();

        if (items.Count > 0)
        {
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
