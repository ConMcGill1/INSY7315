using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    // Page model for displaying detailed information about a product
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _ctx;

        // Get access to the database context
        public DetailsModel(AppDbContext ctx) => _ctx = ctx;

        // Holds the selected product to display on the page
        public Product Product { get; set; } = null!;

        // Runs when the page is loaded (GET request)
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Find the product with the given ID and include its price history
            var p = await _ctx.Products
                .Include(x => x.PriceHistory.OrderByDescending(h => h.ChangedOn))
                .SingleOrDefaultAsync(x => x.Id == id);

            // If product doesn’t exist, show 404 error
            if (p is null) return NotFound();

            // Assign the product and return the page
            Product = p;
            return Page();
        }
    }
}
