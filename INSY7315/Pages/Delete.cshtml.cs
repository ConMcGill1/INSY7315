using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _ctx;
        public DeleteModel(AppDbContext ctx) => _ctx = ctx;

        [BindProperty]
        public Product Product { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var p = await _ctx.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
            if (p is null) return NotFound();
            Product = p;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var entity = await _ctx.Products.SingleOrDefaultAsync(x => x.Id == id);
            if (entity is null) return NotFound();
            _ctx.Products.Remove(entity);
            await _ctx.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
