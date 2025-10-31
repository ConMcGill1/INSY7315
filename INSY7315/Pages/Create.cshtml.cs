using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    // Page model for creating a new product
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _ctx;

        // Gets the database context through dependency injection
        public CreateModel(AppDbContext ctx) => _ctx = ctx;

        // Holds the product data entered in the form
        [BindProperty]
        public Product Product { get; set; } = new();

        // Called when the page is first loaded (GET request)
        public void OnGet() { }

        // Called when the form is submitted (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            // If form validation fails, reload the page with errors
            if (!ModelState.IsValid) return Page();

            // Set the creation time for the new product
            Product.CreatedOn = DateTime.UtcNow;

            // Add the new product to the database
            _ctx.Products.Add(Product);
            await _ctx.SaveChangesAsync();

            // Record the first price entry in the price history
            _ctx.PriceHistories.Add(new PriceHistory
            {
                ProductId = Product.Id,
                OldPrice = Product.Price,
                NewPrice = Product.Price,
                ChangedOn = DateTime.UtcNow
            });
            await _ctx.SaveChangesAsync();

            // Store a success message and return to the main product list
            TempData["Message"] = "Product created.";
            return RedirectToPage("Index");
        }
    }
}
