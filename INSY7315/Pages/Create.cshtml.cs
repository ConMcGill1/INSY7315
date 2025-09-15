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

            _ctx.Products.Add(Product);
            await _ctx.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
