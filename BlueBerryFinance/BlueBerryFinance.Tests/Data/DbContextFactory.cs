using BlueBerryFinance.API.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.Tests.Data
{
    public static class DbContextFactory
    {
        public static AppDbContext Create(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
    }
}
