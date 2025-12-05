namespace EShopOnWeb.Services;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an order confirmation email with receipt.
    /// </summary>
    Task SendOrderConfirmationAsync(EShopOnWeb.Models.Order order);
}
