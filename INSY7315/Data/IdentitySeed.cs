using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace INSY7315.Data
{
    // This class is used to seed default roles and an admin account when the system starts
    public static class IdentitySeed
    {
        // Runs the seed process when called from Program.cs
        public static async Task EnsureSeedAsync(IServiceProvider sp)
        {
            var env = sp.GetRequiredService<IHostEnvironment>();

            // Skip seeding if the app is running in a test environment
            if (env.IsEnvironment("Test") || env.IsEnvironment("Testing")) return;

            // Create a service scope so we can access required services
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Make sure the database is created before seeding users and roles
            try { await db.Database.EnsureCreatedAsync(); } catch { /* ignore any errors here */ }

            // Get the managers needed to create roles and users
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<INSY7315.Models.ApplicationUser>>();

            // Create default roles if they do not already exist
            var roles = new[] { "Admin", "Owner", "Employee" };
            foreach (var role in roles)
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));

            // Create a default admin user if it does not already exist
            var email = "admin@insy7315.local";
            var admin = await userMgr.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (admin == null)
            {
                admin = new INSY7315.Models.ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                // Create the admin user with a preset password and assign roles
                await userMgr.CreateAsync(admin, "Admin#12345");
                await userMgr.AddToRolesAsync(admin, new[] { "Admin", "Owner" });
            }
        }
    }
}
