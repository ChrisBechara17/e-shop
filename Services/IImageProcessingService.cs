namespace EShopOnWeb.Services;

/// <summary>
/// Service for processing product images using AI.
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Processes an uploaded product image to create a clean, consistent product photo.
    /// </summary>
    /// <param name="originalImagePath">Path to the original uploaded image.</param>
    /// <param name="productName">Name of the product for AI context.</param>
    /// <param name="productId">Product ID for file naming.</param>
    /// <returns>URL of the processed image.</returns>
    Task<string> ProcessProductImageAsync(string originalImagePath, string productName, int productId);
}
