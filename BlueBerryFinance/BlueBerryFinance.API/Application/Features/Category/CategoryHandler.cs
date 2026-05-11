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

        public async Task<IReadOnlyList<CategoryViewModel>> GetAsync(
            CategoryFilterRequest filter)
        {
            var query = _db.Categories.AsNoTracking();

            if (filter.Id.HasValue)
                query = query.Where(c => c.Id == filter.Id.Value);

            return await query
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color,
                    Type = c.Type,
                    Active = c.Active == 1
                })
                .ToListAsync();
        }

        public async Task<CategoryViewModel> RegisterAsync(RegisterCategoryRequest request)
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
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register category.", ex);
            }

            return (await GetAsync(new CategoryFilterRequest { Id = entity.Id })).First();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (entity is null) return false;

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete category.", ex);
            }

            return true;
        }
    }
}
