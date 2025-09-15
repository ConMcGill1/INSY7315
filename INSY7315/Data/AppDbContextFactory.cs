using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace INSY7315.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
         
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("Default")
                       ?? "Server=(localdb)\\MSSQLLocalDB;Database=INSY7315;Trusted_Connection=True;MultipleActiveResultSets=true";

            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(conn)
                .Options;

            return new AppDbContext(opts);
        }
    }
}
