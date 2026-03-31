using BlueBerryFinance.API.Application.Handlers;
using BlueBerryFinance.API.Application.Requests.Transaction;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.API.Data.Entities.Enums;
using BlueBerryFinance.Tests.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlueBerryFinance.Tests.API.Handlers
{
    public class TransactionHandlerBalanceTests
    {
        private static async Task<(AppDbContext db, User user, BankAccount account, Store store, Category category, Currency currency)>
            SeedAsync(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<AppDbContext>();

            var currency = new Currency { Code = CurrencyCode.EUR, Symbol = "€", Name = "Euro", Active = 1 };
            currency.SetInsertionDate(DateTime.UtcNow);
            currency.SetLastModification(DateTime.UtcNow);

            var user = new User { Email = "test@test.com", Name = "Test", PasswordHash = "x", Profile = "User", Active = 1 };
            user.SetInsertionDate(DateTime.UtcNow);
            user.SetLastModification(DateTime.UtcNow);

            db.Currencies.Add(currency);
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var account = new BankAccount
            {
                UserId = user.Id,
                CurrencyId = currency.Id,
                Name = "Checking",
                Bank = Bank.ActivoBank,
                Country = Country.PT,
                Balance = 1000m,
                Active = 1
            };
            account.SetInsertionDate(DateTime.UtcNow);
            account.SetLastModification(DateTime.UtcNow);

            var category = new Category { Name = "Food", Icon = "restaurant", Color = "#FF6B35", Type = "Expense", Active = 1 };
            category.SetInsertionDate(DateTime.UtcNow);
            category.SetLastModification(DateTime.UtcNow);

            db.BankAccounts.Add(account);
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var store = new Store { CategoryId = category.Id, Name = "Supermarket", Active = 1 };
            store.SetInsertionDate(DateTime.UtcNow);
            store.SetLastModification(DateTime.UtcNow);

            db.Stores.Add(store);
            await db.SaveChangesAsync();

            return (db, user, account, store, category, currency);
        }

        [Fact]
        public async Task RegisterAsync_Expense_DeductsFromBalance()
        {
            var sp = TestServiceFactory.Build();
            var (db, user, account, store, category, currency) = await SeedAsync(sp);
            var handler = new TransactionHandler(db);

            await handler.RegisterAsync(new RegisterTransactionRequest
            {
                BankAccountId = account.Id,
                StoreId = store.Id,
                CategoryId = category.Id,
                CurrencyId = currency.Id,
                TransactionType = TransactionType.Expense,
                Source = TransactionSource.Manual,
                OriginType = OriginType.Store,
                Amount = 200m,
                Description = "Groceries",
                TransactionDate = DateTime.UtcNow
            }, user.Id);

            var updated = await db.BankAccounts.FindAsync(account.Id);
            updated!.Balance.Should().Be(800m);
        }

        [Fact]
        public async Task RegisterAsync_Income_AddsToBalance()
        {
            var sp = TestServiceFactory.Build();
            var (db, user, account, store, category, currency) = await SeedAsync(sp);
            var handler = new TransactionHandler(db);

            await handler.RegisterAsync(new RegisterTransactionRequest
            {
                BankAccountId = account.Id,
                StoreId = store.Id,
                CategoryId = category.Id,
                CurrencyId = currency.Id,
                TransactionType = TransactionType.Income,
                Source = TransactionSource.Manual,
                OriginType = OriginType.Person,
                Amount = 500m,
                Description = "Salary",
                TransactionDate = DateTime.UtcNow
            }, user.Id);

            var updated = await db.BankAccounts.FindAsync(account.Id);
            updated!.Balance.Should().Be(1500m);
        }

        [Fact]
        public async Task UpdateAsync_ChangesAmount_AdjustsBalanceCorrectly()
        {
            var sp = TestServiceFactory.Build();
            var (db, user, account, store, category, currency) = await SeedAsync(sp);
            var handler = new TransactionHandler(db);

            // Register expense of 100 → balance 900
            var vm = await handler.RegisterAsync(new RegisterTransactionRequest
            {
                BankAccountId = account.Id,
                StoreId = store.Id,
                CategoryId = category.Id,
                CurrencyId = currency.Id,
                TransactionType = TransactionType.Expense,
                Source = TransactionSource.Manual,
                OriginType = OriginType.Store,
                Amount = 100m,
                Description = "Initial",
                TransactionDate = DateTime.UtcNow
            }, user.Id);

            // Update to expense of 300 → balance should be 1000 - 300 = 700
            await handler.UpdateAsync(new UpdateTransactionRequest
            {
                Id = vm.Id,
                StoreId = store.Id,
                CategoryId = category.Id,
                TransactionType = TransactionType.Expense,
                OriginType = OriginType.Store,
                Amount = 300m,
                Description = "Updated",
                TransactionDate = DateTime.UtcNow
            }, user.Id);

            var updated = await db.BankAccounts.FindAsync(account.Id);
            updated!.Balance.Should().Be(700m);
        }

        [Fact]
        public async Task DeleteAsync_Expense_RestoresBalance()
        {
            var sp = TestServiceFactory.Build();
            var (db, user, account, store, category, currency) = await SeedAsync(sp);
            var handler = new TransactionHandler(db);

            // Register expense of 250 → balance 750
            var vm = await handler.RegisterAsync(new RegisterTransactionRequest
            {
                BankAccountId = account.Id,
                StoreId = store.Id,
                CategoryId = category.Id,
                CurrencyId = currency.Id,
                TransactionType = TransactionType.Expense,
                Source = TransactionSource.Manual,
                OriginType = OriginType.Store,
                Amount = 250m,
                Description = "To be deleted",
                TransactionDate = DateTime.UtcNow
            }, user.Id);

            // Delete → balance should go back to 1000
            await handler.DeleteAsync(vm.Id, user.Id);

            var updated = await db.BankAccounts.FindAsync(account.Id);
            updated!.Balance.Should().Be(1000m);
        }
    }
}
