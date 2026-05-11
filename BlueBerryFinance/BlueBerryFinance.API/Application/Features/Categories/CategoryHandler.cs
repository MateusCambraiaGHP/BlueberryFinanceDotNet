using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Categories
{
    public class CategoryHandler : ICategoryHandler
    {
        private readonly AppDbContext _db;

        public CategoryHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<BaseResponse<CategoryViewModel>> GetAsync(CategoryFilterRequest filter)
        {
            var query = _db.Categories.AsNoTracking();

            if (filter.Id.HasValue)
                query = query.Where(c => c.Id == filter.Id.Value);
            
            var result = await query
                .Select(c => ToViewModel(c))
                .ToListAsync();

            return BaseResponse<CategoryViewModel>.Ok(result);
        }

        public async Task<BaseResponse<CategoryViewModel>> CreateAsync(RegisterCategoryRequest request)
        {
            var entity = new Category
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

            var response = await GetAsync(new CategoryFilterRequest { Id = entity.Id });

            return response;
        }

        public async Task<BaseResponse<CategoryViewModel>> DeleteAsync(Guid id)
        {
            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (entity is null) return BaseResponse<CategoryViewModel>.Fail("Category not found");

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

            return BaseResponse<CategoryViewModel>.Ok(ToViewModel(entity));
        }

        private static CategoryViewModel ToViewModel(Category c) => new CategoryViewModel(
           c.Id,
           c.Name,
           c.Icon,
           c.Color,
           c.Type,
           c.Active == 1
           );
    }
}
