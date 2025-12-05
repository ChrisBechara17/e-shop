using EShopOnWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EShopOnWeb.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest(ModelState);
        }

        try
        {
            var order = await _orderService.CreateOrderFromCartAsync(request.SessionId, request.CustomerName, request.CustomerEmail);
            return Ok(order);
        }
        catch (InvalidOperationException)
        {
            return BadRequest("Cart is empty.");
        }
    }

    public class CreateOrderRequest
    {
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public string SessionId { get; set; } = string.Empty;
    }
}
