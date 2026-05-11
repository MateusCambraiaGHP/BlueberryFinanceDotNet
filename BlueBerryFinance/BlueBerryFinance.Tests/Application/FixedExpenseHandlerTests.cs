using BlueBerryFinance.API.Application.Features.FixedExpenses;
using BlueBerryFinance.Tests.Data;
using FluentAssertions;
using Xunit;

namespace BlueBerryFinance.Tests.Application
{
    public class FixedExpenseHandlerTests
    {
        private readonly API.Data.Context.AppDbContext _db;
        private readonly FixedExpenseHandler _handler;

        public FixedExpenseHandlerTests()
        {
            _db      = MockDbContext.GetMock();
            _handler = new FixedExpenseHandler(_db);
        }

        private async Task<API.Domain.Entities.FixedExpense> SeedExpenseAsync(string name = "Netflix", decimal amount = 15m)
        {
            var expense = MockDbContext.BuildFixedExpense(MockDbContext.UserId, name, amount);
            expense.SetInsertionDate(DateTime.UtcNow);
            expense.SetLastModification(DateTime.UtcNow);
            _db.FixedExpenses.Add(expense);
            await _db.SaveChangesAsync();
            return expense;
        }

        [Fact]
        public async Task GetAsync_ReturnsOnlyOwnerExpenses()
        {
            // Arrange
            await SeedExpenseAsync();

            // Act
            var owner = await _handler.GetAsync(new FixedExpenseFilterRequest(), MockDbContext.UserId);
            var other = await _handler.GetAsync(new FixedExpenseFilterRequest(), MockDbContext.OtherUserId);

            // Assert
            owner.Data.Should().HaveCount(1).And.Contain(e => e.Name == "Netflix");
            other.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_CreatesExpenseAndReturnsViewModel()
        {
            // Arrange
            var request = new RegisterFixedExpenseRequest
            {
                CurrencyId  = MockDbContext.CurrencyId,
                StoreId     = MockDbContext.StoreId,
                Name        = "Spotify",
                Description = "Music streaming",
                Amount      = 10m,
                DayOfMonth  = 1,
                IsRecurring = true
            };

            // Act
            var result = await _handler.CreateAsync(request, MockDbContext.UserId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data![0].Name.Should().Be("Spotify");
            result.Data![0].Amount.Should().Be(10m);
            result.Data![0].IsRecurring.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_SoftDeletesExistingExpense_FailsForWrongUserOrMissing()
        {
            // Arrange
            var expense = await SeedExpenseAsync("Gym", 30m);

            // Act
            var wrongUser = await _handler.DeleteAsync(expense.Id, MockDbContext.OtherUserId);
            var success   = await _handler.DeleteAsync(expense.Id, MockDbContext.UserId);
            var notFound  = await _handler.DeleteAsync(Guid.NewGuid(), MockDbContext.UserId);

            // Assert
            wrongUser.Success.Should().BeFalse();
            success.Success.Should().BeTrue();
            _db.FixedExpenses.Find(expense.Id)!.IsDeleted.Should().BeTrue();
            notFound.Success.Should().BeFalse();
            notFound.ValidationErrors.Should().NotBeEmpty();
        }
    }
}
