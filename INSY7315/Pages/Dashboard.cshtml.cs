using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    public class CategoryStat
    {
        public string Category { get; set; } = "";
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    [Authorize(Roles = "Owner,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _ctx;
        public DashboardModel(AppDbContext ctx) => _ctx = ctx;

        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }
        public IList<CategoryStat> TopCategories { get; set; } = new List<CategoryStat>();
        public IList<Alert> RecentAlerts { get; set; } = new List<Alert>();

        public async Task OnGet()
        {
            TotalProducts = await _ctx.Products.CountAsync();
            TotalValue = await _ctx.Products.SumAsync(p => (decimal?)p.Price) ?? 0m;

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

            RecentAlerts = await _ctx.Alerts.AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();
        }
    }
}
