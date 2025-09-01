using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using INSY7315.Data;

namespace INSY7315.Tests;


public class TestAppFactory : WebApplicationFactory<Program>
{
    private string? _dbName;
    private string? _connString;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
         
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

           
            _dbName = "INSY7315_Tests_" + Guid.NewGuid().ToString("N");
            _connString =
                $"Server=(localdb)\\mssqllocaldb;Database={_dbName};Trusted_Connection=True;MultipleActiveResultSets=true";

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(_connString);
            });
        });

        var host = base.CreateHost(builder);

        
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        return host;
    }

   
    public override async ValueTask DisposeAsync()
    {
        try
        {
            if (_connString is not null)
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(_connString)
                    .Options;

                await using var db = new AppDbContext(options);
                await db.Database.EnsureDeletedAsync();
            }
        }
        catch
        {
          
        }

        await base.DisposeAsync();
    }
}
