using EShopOnWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopOnWeb.Data;

/// <summary>
/// EF Core DbContext holding all e-commerce entities and relationships.
/// </summary>
public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.Property(c => c.SessionId).IsRequired();
            entity.Property(c => c.Quantity).HasDefaultValue(1);
            entity.HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CustomerName).IsRequired();
            entity.Property(o => o.CustomerEmail).IsRequired();
            entity.Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
