using EShopOnWeb.Services;
using EShopOnWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EShopOnWeb.Controllers;

public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public OrdersController(IOrderService orderService, ICartService cartService)
    {
        _orderService = orderService;
        _cartService = cartService;
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        return View(new CheckoutViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);

        try
        {
            var order = await _orderService.CreateOrderFromCartAsync(sessionId, model.CustomerName, model.CustomerEmail);
            await _cartService.ClearCartAsync(sessionId);
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return View("NotFound");
        }

        return View(order);
    }
}
