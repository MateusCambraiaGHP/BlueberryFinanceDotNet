using BlueBerryFinance.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Store> Stores => Set<Store>();
        public DbSet<FixedExpense> FixedExpenses => Set<FixedExpense>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<FiscalNote> FiscalNotes => Set<FiscalNote>();
        public DbSet<AgentApproval> AgentApprovals => Set<AgentApproval>();
        public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.Email)
                    .IsUnique();

                e.Property(x => x.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                e.Property(x => x.Name)
                    .HasMaxLength(256)
                    .IsRequired();

                e.Property(x => x.Profile)
                    .HasMaxLength(50)
                    .IsRequired();

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<Currency>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Code).IsRequired();

                e.Property(x => x.Symbol)
                    .HasMaxLength(10)
                    .IsRequired();

                e.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .HasMaxLength(100).IsRequired();

                e.Property(x => x.Icon)
                    .HasMaxLength(100);

                e.Property(x => x.Color)
                    .HasMaxLength(20);

                e.Property(x => x.Type)
                    .HasMaxLength(20)
                    .IsRequired();

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<Store>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .HasMaxLength(256)
                    .IsRequired();

                e.HasOne(x => x.Category)
                    .WithMany(c => c.Stores)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<BankAccount>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .HasMaxLength(256)
                    .IsRequired();

                e.Property(x => x.Balance)
                    .HasPrecision(18, 2);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Currency)
                    .WithMany()
                    .HasForeignKey(x => x.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<FixedExpense>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .HasMaxLength(256)
                    .IsRequired();

                e.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Currency)
                    .WithMany()
                    .HasForeignKey(x => x.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Store)
                    .WithMany()
                    .HasForeignKey(x => x.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<Transaction>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                e.Property(x => x.Description)
                    .HasMaxLength(512);

                e.Property(x => x.CorrelationId)
                    .HasMaxLength(64);

                e.Property(x => x.ImageUrl)
                    .HasMaxLength(1024);

                e.HasIndex(x => x.CorrelationId);

                e.HasIndex(x => new { x.UserId, x.TransactionDate });

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict); 

                e.HasOne(x => x.BankAccount)
                    .WithMany()
                    .HasForeignKey(x => x.BankAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Store)
                    .WithMany()
                    .HasForeignKey(x => x.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                e.HasOne(x => x.Currency)
                    .WithMany()
                    .HasForeignKey(x => x.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.FixedExpense)
                    .WithMany()
                    .HasForeignKey(x => x.FixedExpenseId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<FiscalNote>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.ImageUrl)
                    .HasMaxLength(1024)
                    .IsRequired();

                e.Property(x => x.RawText)
                    .HasMaxLength(8192);

                e.Property(x => x.ExtractedData)
                    .HasColumnType("jsonb");

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Transaction)
                    .WithMany()
                    .HasForeignKey(x => x.TransactionId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<TransactionItem>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .HasMaxLength(512)
                    .IsRequired();

                e.Property(x => x.Quantity)
                    .HasPrecision(18, 4);

                e.Property(x => x.UnitPrice)
                    .HasPrecision(18, 2);

                e.Property(x => x.TotalPrice)
                    .HasPrecision(18, 2);

                e.HasOne(x => x.Transaction)
                    .WithMany(t => t.Items)
                    .HasForeignKey(x => x.TransactionId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasQueryFilter(x => !x.IsDeleted);
            });

            modelBuilder.Entity<AgentApproval>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.AgentName)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(x => x.Tool)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(x => x.Payload)
                    .HasColumnType("jsonb");

                e.HasIndex(x => new { x.UserId, x.Status });

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasQueryFilter(x => !x.IsDeleted);
            });
        }
    }
}
