using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Stores
{
    public class StoreHandler : IStoreHandler
    {
        private readonly AppDbContext _db;

        public StoreHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<BaseResponse<StoreViewModel>> GetAsync(
            StoreFilterRequest filter)
        {
            IQueryable<Store> query = _db.Stores.AsNoTracking().Include(s => s.Category);

            if (filter.Id.HasValue)
                query = query.Where(s => s.Id == filter.Id.Value);

            var entities = await query
                .Select(s => ToViewModel(s))
                .ToListAsync();

            return BaseResponse<StoreViewModel>.Ok(entities);
        }

        public async Task<BaseResponse<StoreViewModel>> CreateAsync(RegisterStoreRequest request)
        {
            var entity = new Store
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

            var response = await GetAsync(new StoreFilterRequest { Id = entity.Id });

            return response;
        }

        public async Task<BaseResponse<StoreViewModel>> DeleteAsync(Guid id)
        {
            var entity = await _db.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (entity is null) return BaseResponse<StoreViewModel>.Fail(new List<string> { "Stores not found" });

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

            return BaseResponse<StoreViewModel>.Ok();
        }

        private static StoreViewModel ToViewModel(Store s) => new StoreViewModel(
            s.Id,
            s.Name,
            s.CategoryId,
            s.Category.Name,
            s.Active == 1);
    }
}
