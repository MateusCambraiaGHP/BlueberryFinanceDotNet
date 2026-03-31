using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlueBerryFinance.API.Data.Context
{
    // Used by EF Core design-time tools (dotnet ef migrations add)
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=localhost;Database=blueberry_finance;Username=blueberry;Password=ChangeMe123!")
                .Options;

            return new AppDbContext(options);
        }
    }
}
