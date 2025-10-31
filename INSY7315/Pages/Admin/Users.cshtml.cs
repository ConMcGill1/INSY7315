using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using INSY7315.Models;

namespace INSY7315.Pages.Admin
{
    // ============================================================
    // ADMIN USER MANAGEMENT PAGE MODEL
    // ------------------------------------------------------------
    // This page is restricted to users in the "Admin" role.
    // Provides functionality to:
    //   • View all registered users.
    //   • View their Display Name and Admin status.
    //   • Promote or demote users between Admin and standard roles.
    // ============================================================
    [Authorize(Roles = "Admin")] // Only Admins can access this page
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly RoleManager<IdentityRole> _roleMgr;

        // Constructor: injects ASP.NET Identity managers for users and roles
        public UsersModel(UserManager<ApplicationUser> userMgr, RoleManager<IdentityRole> roleMgr)
        {
            _userMgr = userMgr;
            _roleMgr = roleMgr;
        }

        // ============================================================
        // RECORD STRUCTURE FOR DISPLAY
        // ------------------------------------------------------------
        // A lightweight projection of ApplicationUser for the UI.
        // Keeps only the fields that are relevant to the admin table.
        // ============================================================
        public record UserRow(string Id, string Email, string? DisplayName, bool IsAdmin);

        // Bound list passed to the Razor page for display
        public IList<UserRow> Users { get; set; } = new List<UserRow>();

        // ============================================================
        // ONGET HANDLER: LOAD USER LIST
        // ------------------------------------------------------------
        // Retrieves all users from the database and determines
        // who currently has the Admin role.
        // Uses AsNoTracking() for efficiency since this data
        // is read-only for display purposes.
        // ============================================================
        public async Task OnGet()
        {
            // Fetch all users without tracking (read-only)
            var allUsers = await _userMgr.Users.AsNoTracking().ToListAsync();

            // Preload Admin role members (to avoid one query per user)
            var admins = new HashSet<string>(StringComparer.Ordinal);
            if (await _roleMgr.RoleExistsAsync("Admin"))
            {
                var adminsInRole = await _userMgr.GetUsersInRoleAsync("Admin");
                foreach (var a in adminsInRole)
                    admins.Add(a.Id);
            }

            // Project each ApplicationUser into a simple record for UI display
            foreach (var u in allUsers)
            {
                // If we preloaded Admin IDs, check locally instead of querying again
                var isAdmin = admins.Count > 0
                    ? admins.Contains(u.Id)
                    : (await _userMgr.IsInRoleAsync(u, "Admin"));

                Users.Add(new UserRow(u.Id, u.Email ?? "", u.DisplayName, isAdmin));
            }
        }

        // ============================================================
        // ONPOST: ADD ADMIN ROLE
        // ------------------------------------------------------------
        // Triggered when an Admin clicks “Add to Admin”.
        // Ensures the Admin role exists, then adds the selected user.
        // Uses TempData to show a confirmation message on redirect.
        // ============================================================
        public async Task<IActionResult> OnPostAddAdminAsync(string id)
        {
            // Find user by ID (hidden input from Razor table)
            var user = await _userMgr.FindByIdAsync(id);
            if (user is null)
                return NotFound(); // Safety check if user deleted mid-request

            // Create the "Admin" role if it doesn't already exist
            if (!await _roleMgr.RoleExistsAsync("Admin"))
                await _roleMgr.CreateAsync(new IdentityRole("Admin"));

            // Add user to Admin role only if not already assigned
            if (!await _userMgr.IsInRoleAsync(user, "Admin"))
                await _userMgr.AddToRoleAsync(user, "Admin");

            // Display confirmation message on reload
            TempData["Message"] = $"Made {user.Email} an Admin.";
            return RedirectToPage(); // Post-Redirect-Get pattern for clean refresh
        }

        // ============================================================
        // ONPOST: REMOVE ADMIN ROLE
        // ------------------------------------------------------------
        // Triggered when an Admin clicks “Remove Admin”.
        // Removes the user’s Admin role assignment if they have one.
        // Uses TempData to notify the admin user.
        // ============================================================
        public async Task<IActionResult> OnPostRemoveAdminAsync(string id)
        {
            var user = await _userMgr.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            // Only attempt to remove if user is currently an Admin
            if (await _userMgr.IsInRoleAsync(user, "Admin"))
                await _userMgr.RemoveFromRoleAsync(user, "Admin");

            TempData["Message"] = $"Removed Admin role from {user.Email}.";
            return RedirectToPage();
        }
    }
}
