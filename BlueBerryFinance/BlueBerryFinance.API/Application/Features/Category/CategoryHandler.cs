using BlueBerryFinance.API.Application.Features.Category;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Category
{
    public class CategoryHandler : ICategoryHandler
    {
        private readonly AppDbContext _db;

        public CategoryHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CategoryViewModel>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Categories
                .AsNoTracking()
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color,
                    Type = c.Type,
                    Active = c.Active == 1
                })
                .ToListAsync(ct);
        }

        public async Task<CategoryViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Categories
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color,
                    Type = c.Type,
                    Active = c.Active == 1
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<CategoryViewModel> RegisterAsync(RegisterCategoryRequest request, CancellationToken ct = default)
        {
            var entity = new Data.Entities.Category
            {
                Name = request.Name,
                Icon = request.Icon,
                Color = request.Color,
                Type = request.Type,
                Active = 1
            };

            entity.SetInsertionDate(DateTime.UtcNow);
            entity.SetLastModification(DateTime.UtcNow);

            _db.Categories.Add(entity);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register category.", ex);
            }

            return (await GetByIdAsync(entity.Id, ct))!;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity is null) return false;

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete category.", ex);
            }

            return true;
        }
    }
}
