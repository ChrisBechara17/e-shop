using EShopOnWeb.Data;
using EShopOnWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopOnWeb.Services;

/// <summary>
/// Product retrieval operations backed by EF Core.
/// </summary>
public class ProductService : IProductService
{
    private readonly ShopDbContext _context;

    public ProductService(ShopDbContext context)
    {
        _context = context;
    }

    public Task<List<Product>> GetAllProductsAsync()
    {
        return _context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public Task<Product?> GetProductByIdAsync(int id)
    {
        return _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
