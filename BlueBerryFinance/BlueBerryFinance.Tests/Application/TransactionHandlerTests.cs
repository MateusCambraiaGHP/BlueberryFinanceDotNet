using BlueBerryFinance.API.Application.Features.Transactions;
using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.Tests.Data;
using FluentAssertions;
using Xunit;

namespace BlueBerryFinance.Tests.Application
{
    public class TransactionHandlerTests
    {
        private readonly API.Data.Context.AppDbContext _db;
        private readonly TransactionHandler _handler;

        public TransactionHandlerTests()
        {
            _db      = MockDbContext.GetMock();
            _handler = new TransactionHandler(_db);
        }

        private RegisterTransactionRequest BuildRequest(
            TransactionType type = TransactionType.Expense,
            decimal amount = 100m) => new()
        {
            BankAccountId   = MockDbContext.BankAccountId,
            StoreId         = MockDbContext.StoreId,
            CategoryId      = MockDbContext.CategoryId,
            CurrencyId      = MockDbContext.CurrencyId,
            OriginType      = OriginType.Store,
            TransactionType = type,
            Amount          = amount,
            Description     = "Test",
            TransactionDate = DateTime.UtcNow
        };

        [Fact]
        public async Task GetAsync_FiltersAndPaginatesCorrectly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            for (var i = 0; i < 5; i++)
            {
                var t = MockDbContext.BuildTransaction(MockDbContext.UserId, MockDbContext.BankAccountId);
                t.TransactionDate = now.AddDays(-i);
                t.SetInsertionDate(now); t.SetLastModification(now);
                _db.Transactions.Add(t);
            }
            var old = MockDbContext.BuildTransaction(MockDbContext.UserId, MockDbContext.BankAccountId);
            old.TransactionDate = now.AddMonths(-2);
            old.SetInsertionDate(now); old.SetLastModification(now);
            _db.Transactions.Add(old);
            await _db.SaveChangesAsync();

            // Act
            var all       = await _handler.GetAsync(new GetTransactionsFilterRequest(), MockDbContext.UserId);
            var paged     = await _handler.GetAsync(new GetTransactionsFilterRequest { Page = 1, PageSize = 3 }, MockDbContext.UserId);
            var otherUser = await _handler.GetAsync(new GetTransactionsFilterRequest(), MockDbContext.OtherUserId);
            var dateRange = await _handler.GetAsync(new GetTransactionsFilterRequest
            {
                DateFrom = now.AddDays(-1),
                DateTo   = now.AddDays(1)
            }, MockDbContext.UserId);

            // Assert
            all.Pagination!.TotalCount.Should().Be(6);
            paged.Data.Should().HaveCount(3);
            paged.Pagination!.TotalCount.Should().Be(6);
            otherUser.Data.Should().BeEmpty();
            dateRange.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateAsync_AdjustsBalanceByType_FailsWhenAccountMissing()
        {
            // Arrange
            var initialBalance = _db.BankAccounts.Find(MockDbContext.BankAccountId)!.Balance;

            // Act
            var expense  = await _handler.CreateAsync(BuildRequest(TransactionType.Expense, 100m), MockDbContext.UserId);
            var income   = await _handler.CreateAsync(BuildRequest(TransactionType.Income, 400m), MockDbContext.UserId);
            var noAcct   = await _handler.CreateAsync(
                new RegisterTransactionRequest { BankAccountId = Guid.NewGuid(), StoreId = MockDbContext.StoreId,
                    CategoryId = MockDbContext.CategoryId, CurrencyId = MockDbContext.CurrencyId,
                    OriginType = OriginType.Store, TransactionType = TransactionType.Expense,
                    Amount = 50m, TransactionDate = DateTime.UtcNow },
                MockDbContext.UserId);

            // Assert
            expense.Success.Should().BeTrue();
            income.Success.Should().BeTrue();
            _db.BankAccounts.Find(MockDbContext.BankAccountId)!.Balance
                .Should().Be(initialBalance - 100m + 400m);
            noAcct.Success.Should().BeFalse();
            noAcct.Message.Should().Contain("Bank account not found");
        }

        [Fact]
        public async Task UpdateAsync_RecalculatesBalance_FailsWhenNotFound()
        {
            // Arrange
            var created = await _handler.CreateAsync(BuildRequest(TransactionType.Expense, 100m), MockDbContext.UserId);
            var balanceAfterCreate = _db.BankAccounts.Find(MockDbContext.BankAccountId)!.Balance;

            // Act
            var updated = await _handler.UpdateAsync(new UpdateTransactionRequest
            {
                Id              = created.Data![0].Id,
                StoreId         = MockDbContext.StoreId,
                CategoryId      = MockDbContext.CategoryId,
                OriginType      = OriginType.Store,
                TransactionType = TransactionType.Expense,
                Amount          = 200m,
                Description     = "Updated",
                TransactionDate = DateTime.UtcNow
            }, MockDbContext.UserId);

            var notFound = await _handler.UpdateAsync(new UpdateTransactionRequest
            {
                Id = Guid.NewGuid(), StoreId = MockDbContext.StoreId, CategoryId = MockDbContext.CategoryId,
                OriginType = OriginType.Store, TransactionType = TransactionType.Expense,
                Amount = 50m, TransactionDate = DateTime.UtcNow
            }, MockDbContext.UserId);

            // Assert
            updated.Success.Should().BeTrue();
            updated.Data![0].Amount.Should().Be(200m);
            updated.Data![0].Description.Should().Be("Updated");
            _db.BankAccounts.Find(MockDbContext.BankAccountId)!.Balance
                .Should().Be(balanceAfterCreate - 100m);
            notFound.Success.Should().BeFalse();
            notFound.Message.Should().Contain("Transaction not found");
        }

        [Fact]
        public async Task DeleteAsync_RevertsBalance_SoftDeletes_FailsWhenNotFoundOrWrongUser()
        {
            // Arrange
            var created = await _handler.CreateAsync(BuildRequest(TransactionType.Expense, 150m), MockDbContext.UserId);
            var balanceAfterCreate = _db.BankAccounts.Find(MockDbContext.BankAccountId)!.Balance;
            var txId = created.Data![0].Id;

            // Act
            var wrongUser = await _handler.DeleteAsync(txId, MockDbContext.OtherUserId);
            var success   = await _handler.DeleteAsync(txId, MockDbContext.UserId);
            var notFound  = await _handler.DeleteAsync(Guid.NewGuid(), MockDbContext.UserId);

            // Assert
            wrongUser.Success.Should().BeFalse();
            success.Success.Should().BeTrue();
            _db.Transactions.Find(txId)!.IsDeleted.Should().BeTrue();
            _db.BankAccounts.Find(MockDbContext.BankAccountId)!.Balance
                .Should().Be(balanceAfterCreate + 150m);
            notFound.Success.Should().BeFalse();
            notFound.Message.Should().Contain("Transaction not found");
        }
    }
}
