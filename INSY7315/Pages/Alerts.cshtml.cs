using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Data;
using INSY7315.Models;

namespace INSY7315.Pages
{
    [Authorize(Roles = "Owner,Admin")]
    public class AlertsModel : PageModel
    {
        private readonly AppDbContext _ctx;
        public AlertsModel(AppDbContext ctx) => _ctx = ctx;

        public IList<Alert> Alerts { get; set; } = new List<Alert>();

        public async Task OnGet()
        {
            Alerts = await _ctx.Alerts.AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();
        }
    }
}
