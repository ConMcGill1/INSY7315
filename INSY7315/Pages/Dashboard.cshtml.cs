using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    // Helper class used to store category statistics for the dashboard
    public class CategoryStat
    {
        public string Category { get; set; } = "";
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    // Only users with Owner or Admin roles can view the dashboard
    [Authorize(Roles = "Owner,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _ctx;

        // Injects the database context so the dashboard can access data
        public DashboardModel(AppDbContext ctx) => _ctx = ctx;

        // Total number of products in the system
        public int TotalProducts { get; set; }

        // Total combined value of all products
        public decimal TotalValue { get; set; }

        // List of top categories with their counts and total values
        public IList<CategoryStat> TopCategories { get; set; } = new List<CategoryStat>();

        // List of the most recent alerts
        public IList<Alert> RecentAlerts { get; set; } = new List<Alert>();

        // Runs when the dashboard page is loaded
        public async Task OnGet()
        {
            // Count the total number of products
            TotalProducts = await _ctx.Products.CountAsync();

            // Calculate the total inventory value (sum of product prices)
            TotalValue = await _ctx.Products.SumAsync(p => (decimal?)p.Price) ?? 0m;

            // Group products by category and get the top 5 with the most items
            TopCategories = await _ctx.Products
                .AsNoTracking()
                .GroupBy(p => p.Category ?? "Uncategorized")
                .Select(g => new CategoryStat
                {
                    Category = g.Key,
                    Count = g.Count(),
                    Value = g.Sum(x => x.Price)
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Get the 5 most recent alerts
            RecentAlerts = await _ctx.Alerts.AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();
        }
    }
}
