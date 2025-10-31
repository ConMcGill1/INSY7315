using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _ctx;
        public CreateModel(AppDbContext ctx) => _ctx = ctx;

        [BindProperty]
        public Product Product { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Product.CreatedOn = DateTime.UtcNow;
            _ctx.Products.Add(Product);
            await _ctx.SaveChangesAsync();

            _ctx.PriceHistories.Add(new PriceHistory
            {
                ProductId = Product.Id,
                OldPrice = Product.Price,
                NewPrice = Product.Price,
                ChangedOn = DateTime.UtcNow
            });
            await _ctx.SaveChangesAsync();

            TempData["Message"] = "Product created.";
            return RedirectToPage("Index");
        }
    }
}
