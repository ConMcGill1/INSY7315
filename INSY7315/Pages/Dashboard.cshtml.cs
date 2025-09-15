using INSY7315.Data;
using INSY7315.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace INSY7315.Pages
{
    [Authorize(Roles = "Owner,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _db;
        public DashboardModel(AppDbContext db) => _db = db;

        public int TotalProducts { get; private set; }
        public decimal TotalInventoryValue { get; private set; }
        public List<Alert> RecentAlerts { get; private set; } = new();
        public List<CategoryRow> TopCategories { get; private set; } = new();

        public class CategoryRow
        {
            public string Category { get; set; } = string.Empty;
            public int Count { get; set; }
            public decimal Value { get; set; }
        }

        public async Task OnGetAsync()
        {
            TotalProducts = await _db.Products.CountAsync();
            TotalInventoryValue = await _db.Products.SumAsync(p => (decimal?)p.Price) ?? 0m;

            RecentAlerts = await _db.Alerts.AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            TopCategories = await _db.Products.AsNoTracking()
                .GroupBy(p => p.Category ?? "Uncategorized")
                .Select(g => new CategoryRow
                {
                    Category = g.Key,
                    Count = g.Count(),
                    Value = g.Sum(p => p.Price)
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();
        }
    }
}
