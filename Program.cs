using System;
using System.Linq;
using EShopOnWeb.Data;
using EShopOnWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string falls back to provided SQL Server instance if none supplied in configuration.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=QuizDB;User Id=admin;Password=admin;Encrypt=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ShopDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IChatbotService, GeminiChatbotService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

var app = builder.Build();

// Apply migrations (when present) or create database, then seed sample data.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

    if (pendingMigrations.Any())
    {
        await context.Database.MigrateAsync();
    }
    else
    {
        await context.Database.EnsureCreatedAsync();
    }

    await DbInitializer.SeedAsync(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API endpoints
app.MapControllers();

app.Run();
