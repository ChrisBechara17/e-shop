using EShopOnWeb.Models;
using EShopOnWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace EShopOnWeb.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatbotService _chatbotService;

    public ChatController(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        var sessionId = SessionHelper.GetOrCreateSessionId(HttpContext);
        var userId = User.Identity?.Name ?? "guest";

        var response = await _chatbotService.ProcessMessageAsync(userId, request.Message, sessionId);
        
        return Ok(response);
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}
