using EShopOnWeb.Data;
using EShopOnWeb.Services;
using EShopOnWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopOnWeb.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ShopDbContext _context;

    public ProductsController(IProductService productService, ShopDbContext context)
    {
        _productService = productService;
        _context = context;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        var products = categoryId.HasValue
            ? await _productService.GetProductsByCategoryAsync(categoryId.Value)
            : await _productService.GetAllProductsAsync();

        var vm = new ProductListViewModel
        {
            Categories = categories,
            Products = products,
            SelectedCategoryId = categoryId
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return View("NotFound");
        }

        return View(product);
    }
}
