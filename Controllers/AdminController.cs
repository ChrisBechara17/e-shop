using Microsoft.AspNetCore.Mvc;
using EShopOnWeb.Data;
using EShopOnWeb.Models;
using EShopOnWeb.Services;
using Microsoft.EntityFrameworkCore;

namespace EShopOnWeb.Controllers;

/// <summary>
/// Admin controller for managing products and processing images.
/// </summary>
public class AdminController : Controller
{
    private readonly ShopDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IImageProcessingService _imageService;

    public AdminController(
        ShopDbContext context, 
        IWebHostEnvironment environment,
        IImageProcessingService imageService)
    {
        _context = context;
        _environment = environment;
        _imageService = imageService;
    }

    /// <summary>
    /// Lists all products for management.
    /// </summary>
    public async Task<IActionResult> Products()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
        
        return View(products);
    }

    /// <summary>
    /// Shows the edit form for a product.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(product);
    }

    /// <summary>
    /// Saves product changes.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> EditProduct(int id, Product model)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.Name = model.Name;
        product.Description = model.Description;
        product.Price = model.Price;
        product.CategoryId = model.CategoryId;

        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Product updated successfully!";
        return RedirectToAction(nameof(Products));
    }

    /// <summary>
    /// Handles image upload and AI processing.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadImage(int productId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an image to upload.";
            return RedirectToAction(nameof(EditProduct), new { id = productId });
        }

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound();

        try
        {
            // Save original uploaded file
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsPath);

            var originalFileName = $"original_{productId}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
            var originalFilePath = Path.Combine(uploadsPath, originalFileName);

            using (var stream = new FileStream(originalFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Process image with AI
            var processedImageUrl = await _imageService.ProcessProductImageAsync(
                originalFilePath, 
                product.Name,
                productId);

            // Update product with new image
            product.ImageUrl = processedImageUrl;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Image processed and saved successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error processing image: {ex.Message}";
        }

        return RedirectToAction(nameof(EditProduct), new { id = productId });
    }

    /// <summary>
    /// Shows form to add a new product.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AddProduct()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(new Product());
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddProduct(Product model)
    {
        if (string.IsNullOrEmpty(model.Name))
        {
            TempData["Error"] = "Product name is required.";
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }

        model.ImageUrl = "https://via.placeholder.com/300x200?text=No+Image";
        _context.Products.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Product created! Now upload an image.";
        return RedirectToAction(nameof(EditProduct), new { id = model.Id });
    }

    /// <summary>
    /// Applies AI-generated images to products based on product names.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ApplyAIImages()
    {
        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "products");
        var appliedCount = 0;

        // Map product names to AI image filenames
        var imageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "4K Monitor", "product_4k_monitor" },
            { "Wireless Headphones", "product_wireless_headphones" },
            { "Yoga Mat", "product_yoga_mat" },
            { "Coffee Maker", "product_coffee_maker" },
            { "Adjustable Dumbbells", "product_dumbbells" },
            { "Blender Pro", "product_blender" }
        };

        var products = await _context.Products.ToListAsync();
        
        foreach (var product in products)
        {
            foreach (var mapping in imageMap)
            {
                if (product.Name.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    // Find the AI image file
                    var files = Directory.GetFiles(uploadsPath, $"{mapping.Value}*.png");
                    if (files.Length > 0)
                    {
                        var fileName = Path.GetFileName(files[0]);
                        product.ImageUrl = $"/uploads/products/{fileName}";
                        appliedCount++;
                        break;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Applied AI images to {appliedCount} products!";
        return RedirectToAction(nameof(Products));
    }
}
