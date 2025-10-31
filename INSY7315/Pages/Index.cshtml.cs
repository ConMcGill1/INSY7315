using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    // Page model for listing and filtering products
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _ctx;
        public IndexModel(AppDbContext ctx) => _ctx = ctx;

        // List of products to display on the page
        public IList<Product> Product { get; set; } = new List<Product>();

        // --- Filter parameters bound from query string ---
        [BindProperty(SupportsGet = true)] public string? Q { get; set; }          // Search text
        [BindProperty(SupportsGet = true)] public string? Category { get; set; }   // Category filter
        [BindProperty(SupportsGet = true)] public string? Model { get; set; }      // Model filter
        [BindProperty(SupportsGet = true)] public decimal? MinPrice { get; set; }  // Minimum price
        [BindProperty(SupportsGet = true)] public decimal? MaxPrice { get; set; }  // Maximum price
        [BindProperty(SupportsGet = true)] public DateTime? CreatedFrom { get; set; } // Created date range (from)
        [BindProperty(SupportsGet = true)] public DateTime? CreatedTo { get; set; }   // Created date range (to)

        // --- Pagination properties ---
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;   // Current page
        public int TotalPages { get; set; }                                           // Total number of pages

        // --- Sorting properties ---
        [BindProperty(SupportsGet = true)] public string? SortBy { get; set; }  // Column to sort by
        [BindProperty(SupportsGet = true)] public string? SortDir { get; set; } // Direction (asc/desc)

        // Called when the page is loaded
        public async Task OnGetAsync()
        {
            const int pageSize = 10; // Number of items per page
            var query = _ctx.Products.AsNoTracking().AsQueryable();

            // --- Apply search filter ---
            if (!string.IsNullOrWhiteSpace(Q))
            {
                var q = Q.Trim();
                query = query.Where(p =>
                    p.Name.Contains(q) ||
                    p.Owner.Contains(q) ||
                    (p.Category != null && p.Category.Contains(q)) ||
                    (p.Model != null && p.Model.Contains(q)) ||
                    p.Id.ToString() == q);
            }

            // --- Apply individual filters ---
            if (!string.IsNullOrWhiteSpace(Category))
                query = query.Where(p => p.Category == Category);

            if (!string.IsNullOrWhiteSpace(Model))
                query = query.Where(p => p.Model == Model);

            if (MinPrice is not null) query = query.Where(p => p.Price >= MinPrice);
            if (MaxPrice is not null) query = query.Where(p => p.Price <= MaxPrice);
            if (CreatedFrom is not null) query = query.Where(p => p.CreatedOn >= CreatedFrom);
            if (CreatedTo is not null) query = query.Where(p => p.CreatedOn <= CreatedTo);

            // --- Sorting setup ---
            var by = (SortBy ?? "name").Trim().ToLowerInvariant();
            var dir = (SortDir ?? "asc").Trim().ToLowerInvariant();
            if (dir != "asc" && dir != "desc") dir = "asc";

            // --- Apply sorting based on selected column ---
            query = (by, dir) switch
            {
                ("price", "asc") => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
                ("price", "desc") => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),

                ("name", "asc") => query.OrderBy(p => p.Name).ThenBy(p => p.Id),
                ("name", "desc") => query.OrderByDescending(p => p.Name).ThenBy(p => p.Id),

                ("owner", "asc") => query.OrderBy(p => p.Owner).ThenBy(p => p.Id),
                ("owner", "desc") => query.OrderByDescending(p => p.Owner).ThenBy(p => p.Id),

                ("category", "asc") => query.OrderBy(p => p.Category).ThenBy(p => p.Id),
                ("category", "desc") => query.OrderByDescending(p => p.Category).ThenBy(p => p.Id),

                ("model", "asc") => query.OrderBy(p => p.Model).ThenBy(p => p.Id),
                ("model", "desc") => query.OrderByDescending(p => p.Model).ThenBy(p => p.Id),

                ("created", "asc") => query.OrderBy(p => p.CreatedOn).ThenBy(p => p.Id),
                ("created", "desc") => query.OrderByDescending(p => p.CreatedOn).ThenBy(p => p.Id),

                ("id", "asc") => query.OrderBy(p => p.Id),
                ("id", "desc") => query.OrderByDescending(p => p.Id),

                _ => query.OrderBy(p => p.Name).ThenBy(p => p.Id)
            };

            // --- Pagination logic ---
            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

            Product = await query
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Determines next sort direction for a column
        public string NextDir(string column) =>
            string.Equals(SortBy, column, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(SortDir, "asc", StringComparison.OrdinalIgnoreCase)
                ? "desc" : "asc";

        // Shows small arrow beside the sorted column
        public string ArrowFor(string column)
        {
            if (!string.Equals(SortBy, column, StringComparison.OrdinalIgnoreCase)) return "";
            return string.Equals(SortDir, "asc", StringComparison.OrdinalIgnoreCase) ? " ▲" : " ▼";
        }
    }
}
