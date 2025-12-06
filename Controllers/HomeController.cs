using EShopOnWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace EShopOnWeb.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;

    public HomeController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        // Redirect unauthenticated users to login
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var featured = (await _productService.GetAllProductsAsync()).Take(4).ToList();
        return View(featured);
    }

    public IActionResult Error()
    {
        return View();
    }
}
