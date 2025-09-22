using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace INSY7315.Data
{
    public static class IdentitySeed
    {
        public static async Task EnsureSeedAsync(IServiceProvider sp)
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            if (env.IsEnvironment("Test") || env.IsEnvironment("Testing")) return;

            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try { await db.Database.EnsureCreatedAsync(); } catch { /* ignore */ }

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<INSY7315.Models.ApplicationUser>>();


            var roles = new[] { "Admin", "Owner", "Employee" };
            foreach (var role in roles)
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));


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
                await userMgr.CreateAsync(admin, "Admin#12345");
                await userMgr.AddToRolesAsync(admin, new[] { "Admin", "Owner" });
            }
        }
    }
}
