using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Store
{
    public class StoreHandler : IStoreHandler
    {
        private readonly AppDbContext _db;

        public StoreHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<StoreViewModel>> GetAsync(
            StoreFilterRequest filter)
        {
            var query = _db.Stores.AsNoTracking();

            if (filter.Id.HasValue)
                query = query.Where(s => s.Id == filter.Id.Value);

            return await query
                .Select(s => new StoreViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.Name,
                    Active = s.Active == 1
                })
                .ToListAsync();
        }

        public async Task<StoreViewModel> RegisterAsync(RegisterStoreRequest request)
        {
            var entity = new Data.Entities.Store
            {
                Name = request.Name,
                CategoryId = request.CategoryId,
                Active = 1
            };

            entity.SetInsertionDate(DateTime.UtcNow);
            entity.SetLastModification(DateTime.UtcNow);

            _db.Stores.Add(entity);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register store.", ex);
            }

            return (await GetAsync(new StoreFilterRequest { Id = entity.Id })).First();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _db.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (entity is null) return false;

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete store.", ex);
            }

            return true;
        }
    }
}
