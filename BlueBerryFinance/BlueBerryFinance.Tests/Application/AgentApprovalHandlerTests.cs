using BlueBerryFinance.API.Application.Features.AgentApprovals;
using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.API.Application.Features.Transactions;
using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.Common.ViewModels;
using BlueBerryFinance.Tests.Data;
using FluentAssertions;
using Moq;
using Xunit;

namespace BlueBerryFinance.Tests.Application
{
    public class AgentApprovalHandlerTests
    {
        private readonly API.Data.Context.AppDbContext _db;
        private readonly Mock<ITransactionHandler> _transactionHandlerMock;
        private readonly AgentApprovalHandler _handler;

        private static readonly string ValidPayload = """
            {
                "bankAccountId": "ffffffff-0000-0000-0000-000000000006",
                "storeId":       "eeeeeeee-0000-0000-0000-000000000005",
                "categoryId":    "dddddddd-0000-0000-0000-000000000004",
                "currencyId":    "cccccccc-0000-0000-0000-000000000003",
                "transactionType": "Expense",
                "originType":      "Store",
                "amount":          45.50,
                "description":     "Groceries",
                "transactionDate": "2025-04-01T10:00:00Z"
            }
            """;

        public AgentApprovalHandlerTests()
        {
            _db                     = MockDbContext.GetMock();
            _transactionHandlerMock = new Mock<ITransactionHandler>();

            _transactionHandlerMock
                .Setup(h => h.CreateAsync(It.IsAny<RegisterTransactionRequest>(), It.IsAny<Guid>()))
                .ReturnsAsync(BaseResponse<TransactionViewModel>.Ok(new TransactionViewModel(
                    Guid.NewGuid(), MockDbContext.UserId,
                    MockDbContext.BankAccountId, "Main Account",
                    MockDbContext.StoreId, "Pingo Doce",
                    MockDbContext.CategoryId, "Food", "#FF5733",
                    "EUR", "€", "Expense", "Manual",
                    50m, "Test", DateTime.UtcNow, null, Guid.NewGuid().ToString(), DateTime.UtcNow)));

            _handler = new AgentApprovalHandler(_db, _transactionHandlerMock.Object);
        }

        private async Task<API.Domain.Entities.AgentApproval> SeedApprovalAsync(
            Guid userId, string tool = "save_income_expense", string? payload = null)
        {
            var approval = MockDbContext.BuildAgentApproval(userId, tool, payload ?? "{}");
            approval.SetInsertionDate(DateTime.UtcNow);
            approval.SetLastModification(DateTime.UtcNow);
            _db.AgentApprovals.Add(approval);
            await _db.SaveChangesAsync();
            return approval;
        }

        [Fact]
        public async Task GetAsync_ReturnsOnlyOwnerPendingApprovals_FiltersByStatus()
        {
            // Arrange
            await SeedApprovalAsync(MockDbContext.UserId);

            // Act
            var owner    = await _handler.GetAsync(MockDbContext.UserId, new AgentApprovalFilterRequest());
            var other    = await _handler.GetAsync(MockDbContext.OtherUserId, new AgentApprovalFilterRequest());
            var approved = await _handler.GetAsync(MockDbContext.UserId,
                               new AgentApprovalFilterRequest { Status = ApprovalStatus.Approved });

            // Assert
            owner.Data.Should().HaveCount(1).And.Contain(a => a.Tool == "save_income_expense");
            other.Data.Should().BeEmpty();
            approved.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task RejectAsync_SetsRejectedStatus_FailsForWrongUserOrMissing()
        {
            // Arrange
            var approval = await SeedApprovalAsync(MockDbContext.UserId);

            // Act
            var wrongUser = await _handler.RejectAsync(approval.Id, MockDbContext.OtherUserId);
            var success   = await _handler.RejectAsync(approval.Id, MockDbContext.UserId);
            var notFound  = await _handler.RejectAsync(Guid.NewGuid(), MockDbContext.UserId);

            // Assert
            wrongUser.Success.Should().BeFalse();
            success.Success.Should().BeTrue();
            _db.AgentApprovals.Find(approval.Id)!.Status.Should().Be(ApprovalStatus.Rejected);
            _db.AgentApprovals.Find(approval.Id)!.ResolvedAt.Should().NotBeNull();
            notFound.Success.Should().BeFalse();
            notFound.ValidationErrors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ApproveAsync_CallsCreateTransactionAndSetsApprovedStatus_FailsForWrongUserOrMissing()
        {
            // Arrange
            var approval = await SeedApprovalAsync(MockDbContext.UserId, "save_income_expense", ValidPayload);

            // Act
            var success  = await _handler.ApproveAsync(approval.Id, MockDbContext.UserId);
            var notFound = await _handler.ApproveAsync(Guid.NewGuid(), MockDbContext.UserId);
            var wrongUser = await SeedApprovalAsync(MockDbContext.UserId, "save_income_expense", ValidPayload)
                .ContinueWith(t => _handler.ApproveAsync(t.Result.Id, MockDbContext.OtherUserId)).Unwrap();

            // Assert
            success.Success.Should().BeTrue();
            _db.AgentApprovals.Find(approval.Id)!.Status.Should().Be(ApprovalStatus.Approved);
            _db.AgentApprovals.Find(approval.Id)!.ResolvedAt.Should().NotBeNull();
            _transactionHandlerMock.Verify(
                h => h.CreateAsync(It.IsAny<RegisterTransactionRequest>(), MockDbContext.UserId), Times.Once);
            notFound.Success.Should().BeFalse();
            notFound.ValidationErrors.Should().NotBeEmpty();
            wrongUser.Success.Should().BeFalse();
        }
    }
}
