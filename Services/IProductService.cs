using EShopOnWeb.Models;

namespace EShopOnWeb.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
}
