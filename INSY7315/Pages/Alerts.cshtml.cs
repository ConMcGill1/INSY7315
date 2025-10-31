using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    // Only users with the Owner or Admin roles can access this page
    [Authorize(Roles = "Owner,Admin")]
    public class AlertsModel : PageModel
    {
        private readonly AppDbContext _ctx;

        // Injects the database context so we can access the Alerts table
        public AlertsModel(AppDbContext ctx) => _ctx = ctx;

        // Holds the list of alerts to show on the page
        public IList<Alert> Alerts { get; set; } = new List<Alert>();

        // Runs when the page loads (GET request)
        public async Task OnGet()
        {
            // Loads the most recent 100 alerts from the database
            // Using AsNoTracking for better performance (read-only data)
            Alerts = await _ctx.Alerts.AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();
        }
    }
}
