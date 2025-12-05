using EShopOnWeb.Models;

namespace EShopOnWeb.Services;

public interface IChatbotService
{
    Task<ChatResponse> ProcessMessageAsync(string userId, string message, string sessionId);
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ChatAction> Actions { get; set; } = new();
}

public class ChatAction
{
    public string Type { get; set; } = string.Empty; // "navigate", "add_to_cart_ui", etc.
    public object Data { get; set; } = new();
}
