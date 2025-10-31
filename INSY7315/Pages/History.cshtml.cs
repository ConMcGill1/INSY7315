using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    // Page model for viewing a product's price history
    public class HistoryModel : PageModel
    {
        private readonly AppDbContext _ctx;

        // Gets access to the database context
        public HistoryModel(AppDbContext ctx) => _ctx = ctx;

        // Holds the product being viewed
        public Product Product { get; set; } = null!;

        // List of past price changes for the product
        public IList<PriceHistory> History { get; set; } = new List<PriceHistory>();

        // Runs when the page is loaded (GET request)
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Find the product by its ID
            var product = await _ctx.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound(); // Show 404 if not found
            Product = product;

            // Get the list of price changes for this product, most recent first
            History = await _ctx.PriceHistories.AsNoTracking()
                .Where(h => h.ProductId == id)
                .OrderByDescending(h => h.ChangedOn)
                .ToListAsync();

            // Show the page with the results
            return Page();
        }
    }
}
