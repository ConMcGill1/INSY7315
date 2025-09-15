using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using INSY7315.Data;

namespace INSY7315.Tests
{
    public class TestAppFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _conn;

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app’s DbContext registration
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Create ONE shared in-memory SQLite connection
                _conn = new SqliteConnection("DataSource=:memory:");
                _conn.Open();

                // Register EF Core with SQLite in-memory
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_conn));

                // Build provider to run migrations/ensure schema
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();   // or db.Database.Migrate() if you prefer
            });

            return base.CreateHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _conn?.Dispose();
                _conn = null;
            }
        }
    }
}
