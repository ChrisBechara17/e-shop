using System.Net.Http.Headers;

namespace EShopOnWeb.Services;

/// <summary>
/// Service that processes product images to create clean, uniform product photos.
/// Integrates with Remove.bg API for professional background removal.
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public ImageProcessingService(
        IWebHostEnvironment environment,
        ILogger<ImageProcessingService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _environment = environment;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<string> ProcessProductImageAsync(
        string originalImagePath, 
        string productName, 
        int productId)
    {
        _logger.LogInformation("Processing image for product: {ProductName}", productName);

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "products");
        var processedFileName = $"product_{productId}_{DateTime.Now.Ticks}.png";
        var processedFilePath = Path.Combine(uploadsPath, processedFileName);

        // Try to use Remove.bg API for background removal
        var apiKey = _configuration["RemoveBg:ApiKey"];
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            try
            {
                var success = await RemoveBackgroundAsync(originalImagePath, processedFilePath, apiKey);
                if (success)
                {
                    _logger.LogInformation("Background removed successfully using Remove.bg API");
                    return $"/uploads/products/{processedFileName}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Remove.bg API failed, falling back to local processing");
            }
        }

        // Fallback: Copy original and apply CSS-based uniform styling
        // The image will be displayed with CSS that provides uniform appearance
        if (File.Exists(originalImagePath))
        {
            File.Copy(originalImagePath, processedFilePath, overwrite: true);
            _logger.LogInformation("Image saved (CSS styling will be applied): {Path}", processedFilePath);
        }

        return $"/uploads/products/{processedFileName}";
    }

    /// <summary>
    /// Calls Remove.bg API to remove background from image.
    /// </summary>
    private async Task<bool> RemoveBackgroundAsync(string inputPath, string outputPath, string apiKey)
    {
        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(inputPath);
        using var fileContent = new StreamContent(fileStream);
        
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "image_file", Path.GetFileName(inputPath));
        form.Add(new StringContent("auto"), "size");
        form.Add(new StringContent("white"), "bg_color"); // White background for uniform look

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await _httpClient.PostAsync("https://api.remove.bg/v1.0/removebg", form);
        
        if (response.IsSuccessStatusCode)
        {
            var resultBytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(outputPath, resultBytes);
            return true;
        }

        var error = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("Remove.bg API error: {Error}", error);
        return false;
    }
}
