using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;
using INSY7315.Services;

namespace INSY7315.Pages
{
    // Page model for editing an existing product
    public class EditModel : PageModel
    {
        private readonly AppDbContext _ctx;
        private readonly PriceChangeService _pcs;

        // Injects the database context and price change service
        public EditModel(AppDbContext ctx, PriceChangeService pcs)
        {
            _ctx = ctx;
            _pcs = pcs;
        }

        // Binds form input to the Product model
        [BindProperty]
        public Product Product { get; set; } = new();

        // Loads the product details when the edit page is opened (GET request)
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var p = await _ctx.Products.FindAsync(id);
            if (p is null) return NotFound(); // If product not found, return 404
            Product = p;
            return Page();
        }

        // Handles form submission (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            // If there are validation errors, redisplay the form
            if (!ModelState.IsValid) return Page();

            // Find the existing product in the database
            var existing = await _ctx.Products.SingleOrDefaultAsync(p => p.Id == Product.Id);
            if (existing is null) return NotFound();

            // Save the old price to compare changes
            var oldPrice = existing.Price;

            // Update product fields with new values
            existing.Name = Product.Name;
            existing.Owner = Product.Owner;
            existing.Category = Product.Category;
            existing.Model = Product.Model;
            existing.Price = Product.Price;

            // If the price was changed, record it in history and trigger alerts
            if (existing.Price != oldPrice)
            {
                _ctx.PriceHistories.Add(new PriceHistory
                {
                    ProductId = existing.Id,
                    OldPrice = oldPrice,
                    NewPrice = existing.Price,
                    ChangedOn = DateTime.UtcNow
                });

                // Handle alerts and price change notifications
                await _pcs.HandlePriceChangeAsync(existing, oldPrice);
            }

            // Save all updates to the database
            await _ctx.SaveChangesAsync();

            // Go back to the main product list
            return RedirectToPage("Index");
        }
    }
}
