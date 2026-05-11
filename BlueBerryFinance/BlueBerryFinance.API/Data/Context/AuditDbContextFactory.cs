using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlueBerryFinance.API.Data.Context
{
    public class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
    {
        public AuditDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile("appsettings.Development.json", optional: true)
               .AddEnvironmentVariables()
               .Build();

            var connectionString = configuration.GetConnectionString("AuditDb")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");

            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AuditDbContext(options);
        }
    }
}
