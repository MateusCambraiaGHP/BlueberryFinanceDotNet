using BlueBerryFinance.API.Application.Features.BankAccounts;
using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.Tests.Data;
using FluentAssertions;
using Xunit;

namespace BlueBerryFinance.Tests.Application
{
    public class BankAccountHandlerTests
    {
        private readonly API.Data.Context.AppDbContext _db;
        private readonly BankAccountHandler _handler;

        public BankAccountHandlerTests()
        {
            _db      = MockDbContext.GetMock();
            _handler = new BankAccountHandler(_db);
        }

        [Fact]
        public async Task GetAsync_ReturnsOnlyOwnerAccounts()
        {
            // Act
            var owner = await _handler.GetAsync(new BankAccountFilterRequest(), MockDbContext.UserId);
            var other = await _handler.GetAsync(new BankAccountFilterRequest(), MockDbContext.OtherUserId);

            // Assert
            owner.Data.Should().HaveCount(1).And.Contain(a => a.Name == "Main Account");
            other.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAsync_FilterById_ReturnsSingleAccount()
        {
            // Act
            var result = await _handler.GetAsync(
                new BankAccountFilterRequest { Id = MockDbContext.BankAccountId }, MockDbContext.UserId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_CreatesAccountWithCorrectBalance()
        {
            // Arrange
            var request = new RegisterBankAccountRequest
            {
                CurrencyId     = MockDbContext.CurrencyId,
                Name           = "Savings",
                Bank           = Bank.Other,
                Country        = Country.Portugal,
                InitialBalance = 500m
            };

            // Act
            var result = await _handler.CreateAsync(request, MockDbContext.UserId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data![0].Name.Should().Be("Savings");
            result.Data![0].Balance.Should().Be(500m);
        }

        [Fact]
        public async Task DeleteAsync_SoftDeletesExistingAccount_FailsForWrongUserOrMissing()
        {
            // Act - valid delete
            var success = await _handler.DeleteAsync(MockDbContext.BankAccountId, MockDbContext.UserId);

            // Act - wrong user
            var wrongUser = await _handler.DeleteAsync(MockDbContext.BankAccountId, MockDbContext.OtherUserId);

            // Act - not found
            var notFound = await _handler.DeleteAsync(Guid.NewGuid(), MockDbContext.UserId);

            // Assert
            success.Success.Should().BeTrue();
            _db.BankAccounts.Find(MockDbContext.BankAccountId)!.IsDeleted.Should().BeTrue();
            wrongUser.Success.Should().BeFalse();
            notFound.Success.Should().BeFalse();
            notFound.ValidationErrors.Should().NotBeEmpty();
        }
    }
}
