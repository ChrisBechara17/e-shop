using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EShopOnWeb.Models;
using Microsoft.Extensions.Configuration;

namespace EShopOnWeb.Services;

public class GeminiChatbotService : IChatbotService
{
    private readonly IConfiguration _configuration;
    private readonly IProductService _productService;
    private readonly ICartService _cartService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiChatbotService> _logger;

    public GeminiChatbotService(
        IConfiguration configuration,
        IProductService productService,
        ICartService cartService,
        HttpClient httpClient,
        ILogger<GeminiChatbotService> logger)
    {
        _configuration = configuration;
        _productService = productService;
        _cartService = cartService;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessMessageAsync(string userId, string message, string sessionId)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey.StartsWith("AIzaSy..."))
        {
            return new ChatResponse { Message = "I am not fully configured yet. Please set the Gemini API Key." };
        }

        // 1. Fetch products to give context (Simple RAG for now)
        var products = await _productService.GetAllProductsAsync();
        var productContext = string.Join("\n", products.Select(p => $"- {p.Name} (ID: {p.Id}): ${p.Price} - {p.Description} [Link: /Products/Details/{p.Id}]"));

        // 2. Prepare prompt
        var systemPrompt = $@"You are a helpful AI assistant for an e-commerce shop called EShopOnWeb.
You have access to the following products in stock:
{productContext}

Your goal is to help the user find products and add them to their cart.
You can perform the following ACTIONS by outputting a specific JSON block at the END of your message:

1. Add to Cart:
{{ ""action"": ""add_to_cart"", ""productId"": 123, ""quantity"": 1, ""productName"": ""Name"" }}

2. Navigate:
{{ ""action"": ""navigate"", ""page"": ""/Cart"" }}

Supported Navigation Paths:
- Home: ""/""
- Cart: ""/Cart""
- Checkout: ""/Cart"" (Redirects to checkout from cart)
- Product Page: ""/Products/Details/ID"" (Use the Link provided in the product list)

If the user asks to add something to cart, confirm which product and then output the JSON action.
If the user asks to see the cart or checkout, output the navigate action.
If the user asks to see a specific product, output the navigate action with the product's Link.
Always be friendly and concise. Do NOT output the JSON if you are just chatting.
Only output ONE action per message.
";

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = systemPrompt + "\n\nUser: " + message } } }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}", content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error: {StatusCode}", response.StatusCode);
            return new ChatResponse { Message = "Sorry, I am having trouble thinking right now." };
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        // Parse simple response
        using var doc = JsonDocument.Parse(jsonResponse);
        var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? string.Empty;

        // 3. Extract Action JSON if present
        var chatResponse = new ChatResponse();
        var actionJson = ExtractActionJson(text);
        
        if (actionJson != null)
        {
            chatResponse.Message = text.Replace(actionJson, "").Trim();
            
            try 
            {
                var actionData = JsonDocument.Parse(actionJson);
                var actionType = actionData.RootElement.GetProperty("action").GetString();
                
                if (actionType == "add_to_cart")
                {
                    var productId = actionData.RootElement.GetProperty("productId").GetInt32();
                    var qt = actionData.RootElement.TryGetProperty("quantity", out var q) ? q.GetInt32() : 1;
                    
                    // Execute server-side logic
                    await _cartService.AddToCartAsync(sessionId, productId, qt);
                    
                    chatResponse.Actions.Add(new ChatAction 
                    { 
                        Type = "add_to_cart", 
                        Data = new { productId, quantity = qt, productName = actionData.RootElement.GetProperty("productName").GetString() } 
                    });
                }
                else if (actionType == "navigate")
                {
                    chatResponse.Actions.Add(new ChatAction 
                    { 
                        Type = "navigate", 
                        Data = new { url = actionData.RootElement.GetProperty("page").GetString() } 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing chat action");
            }
        }
        else
        {
            chatResponse.Message = text;
        }

        return chatResponse;
    }

    private string? ExtractActionJson(string? text)
    {
        if (string.IsNullOrEmpty(text)) return null;

        var startIndex = text.LastIndexOf("{");
        var endIndex = text.LastIndexOf("}");
        
        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            var potentialJson = text.Substring(startIndex, endIndex - startIndex + 1);
            if (potentialJson.Contains("\"action\""))
            {
                return potentialJson;
            }
        }
        return null;
    }
}
