using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EShopOnWeb.Services;

/// <summary>
/// SMTP email service for sending order receipts.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(EShopOnWeb.Models.Order order)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUser = _configuration["Smtp:Username"];
        var smtpPass = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"] ?? smtpUser;

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
        {
            _logger.LogWarning("SMTP not configured. Skipping email for order {OrderId}", order.Id);
            return;
        }

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var html = BuildEmailHtml(order);

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail!, "EShopOnWeb"),
                Subject = $"Order Confirmation - #{order.Id}",
                Body = html,
                IsBodyHtml = true
            };
            message.To.Add(order.CustomerEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Order confirmation email sent to {Email} for order {OrderId}", 
                order.CustomerEmail, order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", order.Id);
        }
    }

    private string BuildEmailHtml(EShopOnWeb.Models.Order order)
    {
        var sb = new StringBuilder();
        
        sb.Append(@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; background: #f8fafc; }
        .container { max-width: 600px; margin: 0 auto; background: white; }
        .header { background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%); color: white; padding: 2rem; text-align: center; }
        .header h1 { margin: 0; font-size: 1.5rem; }
        .content { padding: 2rem; }
        .success-icon { font-size: 3rem; margin-bottom: 1rem; }
        .order-details { margin: 1.5rem 0; }
        .order-item { display: flex; justify-content: space-between; padding: 0.75rem 0; border-bottom: 1px solid #e2e8f0; }
        .total { font-size: 1.25rem; font-weight: bold; color: #6366f1; padding-top: 1rem; }
        .footer { background: #1e293b; color: #94a3b8; padding: 1.5rem; text-align: center; font-size: 0.875rem; }
        .btn { display: inline-block; background: #6366f1; color: white; padding: 0.75rem 1.5rem; border-radius: 0.5rem; text-decoration: none; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>✓</div>
            <h1>Thank You for Your Order!</h1>
        </div>
        <div class='content'>
            <p>Hi " + order.CustomerName + @",</p>
            <p>Your order has been confirmed and is being processed. Here's a summary:</p>
            
            <div class='order-details'>
                <p><strong>Order #" + order.Id + @"</strong><br>
                Date: " + order.CreatedAt.ToString("MMMM dd, yyyy") + @"</p>
                
                <div style='margin-top: 1rem;'>");

        foreach (var item in order.Items)
        {
            sb.Append($@"
                    <div class='order-item'>
                        <span>{item.Product?.Name ?? "Product"} × {item.Quantity}</span>
                        <span>${(item.Quantity * item.UnitPrice):F2}</span>
                    </div>");
        }

        sb.Append(@"
                </div>
                
                <div class='total' style='text-align: right; border-top: 2px solid #e2e8f0;'>
                    Total: $" + order.TotalAmount.ToString("F2") + @"
                </div>
            </div>
            
            <p style='text-align: center; margin-top: 2rem;'>
                <a href='#' class='btn'>Track Your Order</a>
            </p>
        </div>
        <div class='footer'>
            <p>© 2024 EShopOnWeb. All rights reserved.</p>
            <p>If you have any questions, reply to this email.</p>
        </div>
    </div>
</body>
</html>");

        return sb.ToString();
    }
}
