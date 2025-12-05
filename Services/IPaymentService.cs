namespace EShopOnWeb.Services;

/// <summary>
/// Service for processing payments via Stripe.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a Stripe Checkout session for the given order.
    /// </summary>
    /// <param name="order">The order to pay for.</param>
    /// <param name="successUrl">URL to redirect after successful payment.</param>
    /// <param name="cancelUrl">URL to redirect if payment is cancelled.</param>
    /// <returns>Stripe Checkout session URL.</returns>
    Task<string> CreateCheckoutSessionAsync(EShopOnWeb.Models.Order order, string successUrl, string cancelUrl);
}
