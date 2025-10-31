using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace INSY7315.Data
{
    // This factory class is used when creating the database context at design time
    // It helps with tasks like running migrations outside of the main program
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Loads configuration files to get the connection string
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Gets the database connection string from configuration
            // If it's missing, a default local connection is used
            var conn = config.GetConnectionString("Default")
                       ?? "Server=(localdb)\\MSSQLLocalDB;Database=INSY7315;Trusted_Connection=True;MultipleActiveResultSets=true";

            // Builds the options for the AppDbContext using SQL Server
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(conn)
                .Options;

            // Returns a new instance of the AppDbContext using the options
            return new AppDbContext(opts);
        }
    }
}
