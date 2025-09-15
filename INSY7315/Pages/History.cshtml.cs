using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    public class HistoryModel : PageModel
    {
        private readonly AppDbContext _ctx;
        public HistoryModel(AppDbContext ctx) => _ctx = ctx;

        public Product Product { get; set; } = null!;
        public IList<PriceHistory> History { get; set; } = new List<PriceHistory>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _ctx.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            Product = product;

            History = await _ctx.PriceHistories.AsNoTracking()
                .Where(h => h.ProductId == id)
                .OrderByDescending(h => h.ChangedOn)
                .ToListAsync();

            return Page();
        }
    }
}
