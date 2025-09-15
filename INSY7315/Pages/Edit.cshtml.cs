using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;
using INSY7315.Services;

namespace INSY7315.Pages
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _ctx;
        private readonly PriceChangeService _pcs;
        public EditModel(AppDbContext ctx, PriceChangeService pcs) { _ctx = ctx; _pcs = pcs; }

        [BindProperty]
        public Product Product { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var p = await _ctx.Products.FindAsync(id);
            if (p is null) return NotFound();
            Product = p;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var existing = await _ctx.Products.SingleOrDefaultAsync(p => p.Id == Product.Id);
            if (existing is null) return NotFound();

            var oldPrice = existing.Price;

            existing.Name = Product.Name;
            existing.Owner = Product.Owner;
            existing.Category = Product.Category;
            existing.Model = Product.Model;
            existing.Price = Product.Price;

            if (existing.Price != oldPrice)
            {
                _ctx.PriceHistories.Add(new PriceHistory
                {
                    ProductId = existing.Id,
                    OldPrice = oldPrice,
                    NewPrice = existing.Price,
                    ChangedOn = DateTime.UtcNow
                });
                await _pcs.HandlePriceChangeAsync(existing, oldPrice);
            }

            await _ctx.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
