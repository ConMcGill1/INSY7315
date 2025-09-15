using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _ctx;
        public DetailsModel(AppDbContext ctx) => _ctx = ctx;

        public Product Product { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var p = await _ctx.Products
                .Include(x => x.PriceHistory.OrderByDescending(h => h.ChangedOn))
                .SingleOrDefaultAsync(x => x.Id == id);
            if (p is null) return NotFound();
            Product = p;
            return Page();
        }
    }
}
