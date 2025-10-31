using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    // Page model for deleting a product
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _ctx;

        // Get access to the database context
        public DeleteModel(AppDbContext ctx) => _ctx = ctx;

        // Holds the product to be deleted
        [BindProperty]
        public Product Product { get; set; } = null!;

        // Runs when the page is loaded (GET request)
        // Finds the product by ID and shows it to confirm deletion
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var p = await _ctx.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
            if (p is null) return NotFound(); // If no product found, show 404 page
            Product = p;
            return Page();
        }

        // Runs when the delete form is submitted (POST request)
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var entity = await _ctx.Products.SingleOrDefaultAsync(x => x.Id == id);
            if (entity is null) return NotFound(); // If product was already removed
            _ctx.Products.Remove(entity); // Remove the product
            await _ctx.SaveChangesAsync(); // Save changes to the database
            return RedirectToPage("Index"); // Go back to the product list
        }
    }
}
