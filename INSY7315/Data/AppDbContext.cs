using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using INSY7315.Models;

namespace INSY7315.Data
{

    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
        public DbSet<Alert> Alerts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Product>()
             .HasMany(p => p.PriceHistory)
             .WithOne(h => h.Product)
             .HasForeignKey(h => h.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Product>()
             .HasIndex(p => new { p.Name, p.Category, p.Model });
        }
    }
}
