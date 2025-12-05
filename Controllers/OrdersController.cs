using EShopOnWeb.Data;
using EShopOnWeb.Services;
using EShopOnWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopOnWeb.Controllers;

public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ShopDbContext _context;

    public OrdersController(
        IOrderService orderService, 
        ICartService cartService,
        IPaymentService paymentService,
        IEmailService emailService,
        IConfiguration configuration,
        ShopDbContext context)
    {
        _orderService = orderService;
        _cartService = cartService;
        _paymentService = paymentService;
        _emailService = emailService;
        _configuration = configuration;
        _context = context;
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
        return View(new CheckoutViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            return View(model);
        }

        var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);

        try
        {
            // Create order from cart
            var order = await _orderService.CreateOrderFromCartAsync(sessionId, model.CustomerName, model.CustomerEmail);
            
            // Check if Stripe is configured
            var stripeKey = _configuration["Stripe:SecretKey"];
            if (!string.IsNullOrEmpty(stripeKey))
            {
                try
                {
                    // Load the order with items for Stripe
                    var orderWithItems = await _context.Orders
                        .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(o => o.Id == order.Id);
                    
                    if (orderWithItems != null && orderWithItems.Items.Any())
                    {
                        // Use configured AppUrl or fallback to request URL
                        var baseUrl = _configuration["AppUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                        
                        // Redirect to Stripe Checkout
                        var successUrl = $"{baseUrl}/Orders/PaymentSuccess";
                        var cancelUrl = $"{baseUrl}/Orders/PaymentCancelled?orderId={order.Id}";
                        
                        var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(orderWithItems, successUrl, cancelUrl);
                        
                        // Store order ID in session for later retrieval
                        HttpContext.Session.SetInt32("PendingOrderId", order.Id);
                        
                        return Redirect(checkoutUrl);
                    }
                }
                catch (Exception ex)
                {
                    // Log Stripe error and fallback to direct order
                    Console.WriteLine($"Stripe error: {ex.Message}");
                }
            }
            
            // No Stripe or Stripe failed - proceed directly
            await _cartService.ClearCartAsync(sessionId);
            
            // Load order for email
            var orderForEmail = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);
            
            if (orderForEmail != null)
            {
                await _emailService.SendOrderConfirmationAsync(orderForEmail);
            }
            
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty.");
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            return View(model);
        }
    }

    /// <summary>
    /// Stripe payment success callback.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> PaymentSuccess(string session_id)
    {
        var orderId = HttpContext.Session.GetInt32("PendingOrderId");
        if (orderId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);
        
        // Clear cart after successful payment
        await _cartService.ClearCartAsync(sessionId);
        HttpContext.Session.Remove("PendingOrderId");

        // Get order with items for email
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order != null)
        {
            // Send confirmation email
            await _emailService.SendOrderConfirmationAsync(order);
        }

        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    /// <summary>
    /// Stripe payment cancelled callback.
    /// </summary>
    [HttpGet]
    public IActionResult PaymentCancelled(int orderId)
    {
        HttpContext.Session.Remove("PendingOrderId");
        TempData["Error"] = "Payment was cancelled. Your order has not been processed.";
        return RedirectToAction("Index", "Cart");
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
