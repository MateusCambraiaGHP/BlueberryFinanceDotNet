using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.API.Domain.Entities.Enums;

namespace BlueBerryFinance.Tests.Data
{
    public static class SeedData
    {
        public static readonly Guid UserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
        public static readonly Guid OtherUserId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
        public static readonly Guid CurrencyId = Guid.Parse("cccccccc-0000-0000-0000-000000000003");
        public static readonly Guid CategoryId = Guid.Parse("dddddddd-0000-0000-0000-000000000004");
        public static readonly Guid StoreId = Guid.Parse("eeeeeeee-0000-0000-0000-000000000005");
        public static readonly Guid BankAccountId = Guid.Parse("ffffffff-0000-0000-0000-000000000006");

        public static Currency Currency() => new()
        {
            Id = CurrencyId,
            Code = CurrencyCode.EUR,
            Symbol = "€",
            Name = "Euro",
            Active = 1
        };

        public static Category Category() => new()
        {
            Id = CategoryId,
            Name = "Food",
            Icon = "restaurant",
            Color = "#FF5733",
            Type = "Expense",
            Active = 1
        };

        public static Store Store() => new()
        {
            Id = StoreId,
            Name = "Pingo Doce",
            CategoryId = CategoryId,
            Active = 1
        };

        public static BankAccount BankAccount(decimal balance = 1000m) => new()
        {
            Id = BankAccountId,
            UserId = UserId,
            CurrencyId = CurrencyId,
            Name = "Main Account",
            Bank = Bank.Other,
            Country = Country.Portugal,
            Balance = balance,
            Active = 1
        };

        public static async Task SeedAsync(AppDbContext db)
        {
            var now = DateTime.UtcNow;

            var currency = Currency();
            currency.SetInsertionDate(now);
            currency.SetLastModification(now);

            var category = Category();
            category.SetInsertionDate(now);
            category.SetLastModification(now);

            var store = Store();
            store.SetInsertionDate(now);
            store.SetLastModification(now);

            var bankAccount = BankAccount();
            bankAccount.SetInsertionDate(now);
            bankAccount.SetLastModification(now);

            db.Currencies.Add(currency);
            db.Categories.Add(category);
            db.Stores.Add(store);
            db.BankAccounts.Add(bankAccount);

            await db.SaveChangesAsync();
        }

        public static Transaction Transaction(
            Guid userId,
            Guid bankAccountId,
            TransactionType type = TransactionType.Expense,
            decimal amount = 50m) => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BankAccountId = bankAccountId,
            StoreId = StoreId,
            CategoryId = CategoryId,
            CurrencyId = CurrencyId,
            OriginType = OriginType.Store,
            TransactionType = type,
            Source = TransactionSource.Manual,
            Amount = amount,
            Description = "Test transaction",
            TransactionDate = DateTime.UtcNow,
            Active = 1
        };
    }
}
