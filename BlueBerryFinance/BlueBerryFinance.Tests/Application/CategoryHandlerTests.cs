using BlueBerryFinance.API.Application.Features.Categories;
using BlueBerryFinance.Tests.Data;
using FluentAssertions;
using Xunit;

namespace BlueBerryFinance.Tests.Application
{
    public class CategoryHandlerTests
    {
        private readonly API.Data.Context.AppDbContext _db;
        private readonly CategoryHandler _handler;

        public CategoryHandlerTests()
        {
            _db      = MockDbContext.GetMock();
            _handler = new CategoryHandler(_db);
        }

        [Fact]
        public async Task GetAsync_ReturnsAllAndFiltersById()
        {
            // Act
            var all    = await _handler.GetAsync(new CategoryFilterRequest());
            var byId   = await _handler.GetAsync(new CategoryFilterRequest { Id = MockDbContext.CategoryId });
            var noMatch = await _handler.GetAsync(new CategoryFilterRequest { Id = Guid.NewGuid() });

            // Assert
            all.Data.Should().HaveCount(1).And.Contain(c => c.Name == "Food");
            byId.Data.Should().HaveCount(1).And.Contain(c => c.Color == "#FF5733");
            noMatch.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_CreatesCategoryAndReturnsViewModel()
        {
            // Arrange
            var request = new RegisterCategoryRequest
            {
                Name  = "Transport",
                Icon  = "directions_car",
                Color = "#3498DB",
                Type  = "Expense"
            };

            // Act
            var result = await _handler.CreateAsync(request);

            // Assert
            result.Success.Should().BeTrue();
            result.Data![0].Name.Should().Be("Transport");
            result.Data![0].Color.Should().Be("#3498DB");
            result.Data![0].Active.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_SoftDeletesExistingCategory_FailsWhenNotFound()
        {
            // Act
            var success  = await _handler.DeleteAsync(MockDbContext.CategoryId);
            var notFound = await _handler.DeleteAsync(Guid.NewGuid());

            // Assert
            success.Success.Should().BeTrue();
            _db.Categories.Find(MockDbContext.CategoryId)!.IsDeleted.Should().BeTrue();
            notFound.Success.Should().BeFalse();
        }
    }
}
