using EShopOnWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopOnWeb.Data;

/// <summary>
/// Seeds a handful of categories and products for demo purposes.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(ShopDbContext context)
    {
        if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Books" },
            new() { Name = "Electronics" },
            new() { Name = "Home & Kitchen" },
            new() { Name = "Fitness" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new() { Name = "C# in Depth", Description = "Deep dive into C# programming techniques.", Price = 49.99m, CategoryId = categories[0].Id, ImageUrl = "https://via.placeholder.com/300x200?text=C%23+Book" },
            new() { Name = "ASP.NET Core Unleashed", Description = "Comprehensive guide for ASP.NET Core developers.", Price = 59.99m, CategoryId = categories[0].Id, ImageUrl = "https://via.placeholder.com/300x200?text=ASP.NET+Book" },
            new() { Name = "Noise Cancelling Headphones", Description = "Wireless over-ear headphones with ANC.", Price = 199.99m, CategoryId = categories[1].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Headphones" },
            new() { Name = "4K Monitor", Description = "27-inch UHD monitor with HDR.", Price = 329.99m, CategoryId = categories[1].Id, ImageUrl = "https://via.placeholder.com/300x200?text=4K+Monitor" },
            new() { Name = "Smart Speaker", Description = "Voice assistant for your smart home.", Price = 99.99m, CategoryId = categories[1].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Smart+Speaker" },
            new() { Name = "Chef Knife Set", Description = "Professional stainless steel knives.", Price = 89.99m, CategoryId = categories[2].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Knife+Set" },
            new() { Name = "Espresso Machine", Description = "Compact espresso machine for home baristas.", Price = 249.99m, CategoryId = categories[2].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Espresso+Machine" },
            new() { Name = "Air Fryer", Description = "Healthier frying with little to no oil.", Price = 129.99m, CategoryId = categories[2].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Air+Fryer" },
            new() { Name = "Yoga Mat", Description = "Non-slip mat for yoga and workouts.", Price = 29.99m, CategoryId = categories[3].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Yoga+Mat" },
            new() { Name = "Adjustable Dumbbells", Description = "Space-saving dumbbell pair with quick weight changes.", Price = 199.99m, CategoryId = categories[3].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Dumbbells" },
            new() { Name = "Resistance Bands", Description = "Set of resistance bands for strength training.", Price = 24.99m, CategoryId = categories[3].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Resistance+Bands" },
            new() { Name = "Stainless Steel Water Bottle", Description = "Insulated bottle to keep drinks cold or hot.", Price = 19.99m, CategoryId = categories[3].Id, ImageUrl = "https://via.placeholder.com/300x200?text=Water+Bottle" }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
