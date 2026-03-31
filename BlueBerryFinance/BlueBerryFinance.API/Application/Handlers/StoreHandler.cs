using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests.Store;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Handlers
{
    public class StoreHandler : IStoreHandler
    {
        private readonly AppDbContext _db;

        public StoreHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<StoreViewModel>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Stores
                .AsNoTracking()
                .Select(s => new StoreViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.Name,
                    Active = s.Active == 1
                })
                .ToListAsync(ct);
        }

        public async Task<StoreViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Stores
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new StoreViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.Name,
                    Active = s.Active == 1
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<StoreViewModel> RegisterAsync(RegisterStoreRequest request, CancellationToken ct = default)
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
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register store.", ex);
            }

            return (await GetByIdAsync(entity.Id, ct))!;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Stores.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (entity is null) return false;

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete store.", ex);
            }

            return true;
        }
    }
}
