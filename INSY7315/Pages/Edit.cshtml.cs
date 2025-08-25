using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    public class EditModel : PageModel
    {
        private readonly INSY7315.Data.AppDbContext _context;

        public EditModel(INSY7315.Data.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product =  await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            Product = product;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(Product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            var existing = await _context.Products
    .Include(p => p.PriceHistory)
    .FirstOrDefaultAsync(p => p.Id == Product.Id);

            if (existing is null) return NotFound();
            if (!ModelState.IsValid) return Page();

            if (existing.Price != Product.Price)
            {
                _context.PriceHistories.Add(new INSY7315.Models.PriceHistory
                {
                    ProductId = existing.Id,
                    OldPrice = existing.Price,
                    NewPrice = Product.Price,
                    ChangedOn = DateTime.UtcNow
                });
                existing.Price = Product.Price;
            }

            existing.Name = Product.Name;
            existing.Owner = Product.Owner;
            existing.Model = Product.Model;
            existing.Category = Product.Category;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");


           
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
