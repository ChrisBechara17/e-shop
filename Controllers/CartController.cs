using EShopOnWeb.Services;
using EShopOnWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EShopOnWeb.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductService _productService;

    public CartController(ICartService cartService, IProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);
        var items = await _cartService.GetCartItemsAsync(sessionId);
        var vm = new CartViewModel { Items = items };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return View("NotFound");
        }

        var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);
        await _cartService.AddToCartAsync(sessionId, productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);
        await _cartService.RemoveFromCartAsync(sessionId, cartItemId);
        return RedirectToAction(nameof(Index));
    }
}
