using Microsoft.EntityFrameworkCore;
using INSY7315.Models;

namespace INSY7315.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>()
         .HasMany(p => p.PriceHistory)
         .WithOne(h => h.Product)
         .HasForeignKey(h => h.ProductId)
         .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Product>()
         .HasIndex(p => new { p.Name, p.Category, p.Model });
    }
}
