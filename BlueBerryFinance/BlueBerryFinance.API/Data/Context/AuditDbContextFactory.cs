using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlueBerryFinance.API.Data.Context
{
    public class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
    {
        public AuditDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseNpgsql("Host=localhost;Database=blueberry_audit;Username=blueberry;Password=ChangeMe123!")
                .Options;

            return new AuditDbContext(options);
        }
    }
}
