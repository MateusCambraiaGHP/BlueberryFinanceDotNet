using BlueBerryFinance.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Data.Context
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.CorrelationId).HasMaxLength(64);
                e.Property(x => x.Controller).HasMaxLength(100);
                e.Property(x => x.Action).HasMaxLength(100);
                e.Property(x => x.Method).HasMaxLength(10);
                e.Property(x => x.Payload).HasColumnType("jsonb");
                e.Property(x => x.Response).HasColumnType("jsonb");
                e.HasIndex(x => new { x.UserId, x.InsertionDate });
                e.HasIndex(x => x.CorrelationId);
                e.HasIndex(x => x.StatusCode).HasFilter("\"StatusCode\" >= 400");
            });
        }
    }
}
