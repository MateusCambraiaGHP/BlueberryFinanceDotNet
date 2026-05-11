using BlueBerryFinance.API.Application.Features.Category;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities;
using BlueBerryFinance.Tests.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlueBerryFinance.Tests.API.Handlers
{
    public class CategoryHandlerTests
    {
        [Fact]
        public async Task RegisterAsync_WithValidRequest_ReturnsViewModel()
        {
            var sp = TestServiceFactory.Build();
            var db = sp.GetRequiredService<AppDbContext>();
            var handler = new CategoryHandler(db);

            var request = new RegisterCategoryRequest
            {
                Name = "Food",
                Icon = "restaurant",
                Color = "#FF6B35",
                Type = "Expense"
            };

            var result = await handler.RegisterAsync(request);

            result.Should().NotBeNull();
            result.Name.Should().Be("Food");
            result.Type.Should().Be("Expense");
            result.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ListAsync_ReturnsOnlyActiveCategories()
        {
            var sp = TestServiceFactory.Build();
            var db = sp.GetRequiredService<AppDbContext>();

            var active = new Category { Name = "Rent", Icon = "home", Color = "#4f8ef7", Type = "Expense", Active = 1 };
            var deleted = new Category { Name = "Old", Icon = "delete", Color = "#ff0000", Type = "Expense", Active = 0 };
            active.SetInsertionDate(DateTime.UtcNow);
            active.SetLastModification(DateTime.UtcNow);
            deleted.SetInsertionDate(DateTime.UtcNow);
            deleted.SetLastModification(DateTime.UtcNow);
            deleted.Delete();

            db.Categories.AddRange(active, deleted);
            await db.SaveChangesAsync();

            var handler = new CategoryHandler(db);
            var result = await handler.ListAsync();

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Rent");
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_SoftDeletes()
        {
            var sp = TestServiceFactory.Build();
            var db = sp.GetRequiredService<AppDbContext>();
            var handler = new CategoryHandler(db);

            var created = await handler.RegisterAsync(new RegisterCategoryRequest
            {
                Name = "Transport",
                Icon = "car",
                Color = "#00b4d8",
                Type = "Expense"
            });

            var deleted = await handler.DeleteAsync(created.Id);
            deleted.Should().BeTrue();

            var found = await handler.GetByIdAsync(created.Id);
            found.Should().BeNull(); // soft deleted, filtered by global query filter
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
        {
            var sp = TestServiceFactory.Build();
            var db = sp.GetRequiredService<AppDbContext>();
            var handler = new CategoryHandler(db);

            var result = await handler.DeleteAsync(Guid.NewGuid());
            result.Should().BeFalse();
        }
    }
}
