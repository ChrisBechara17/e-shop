using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace EShopOnWeb.Services;

/// <summary>
/// Stripe payment processing service.
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Configure Stripe with secret key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<string> CreateCheckoutSessionAsync(
        EShopOnWeb.Models.Order order, 
        string successUrl, 
        string cancelUrl)
    {
        _logger.LogInformation("Creating Stripe checkout session for order {OrderId}", order.Id);

        // Get base URL for absolute image paths
        var baseUrl = _configuration["AppUrl"] ?? "";
        
        var lineItems = order.Items.Select(item => {
            // Build absolute image URL if product has an image
            List<string>? images = null;
            if (!string.IsNullOrEmpty(item.Product?.ImageUrl))
            {
                var imageUrl = item.Product.ImageUrl;
                if (!imageUrl.StartsWith("http"))
                {
                    imageUrl = baseUrl + imageUrl;
                }
                images = new List<string> { imageUrl };
            }
            
            return new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product?.Name ?? "Product",
                        Description = item.Product?.Description ?? "",
                        Images = images
                    },
                    UnitAmount = (long)(item.UnitPrice * 100), // Stripe uses cents
                },
                Quantity = item.Quantity,
            };
        }).ToList();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            CustomerEmail = order.CustomerEmail,
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.Id.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation("Created Stripe session {SessionId} for order {OrderId}", session.Id, order.Id);
        return session.Url;
    }
}
