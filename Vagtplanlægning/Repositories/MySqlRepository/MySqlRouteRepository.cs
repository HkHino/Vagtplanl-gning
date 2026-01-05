using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MySqlRouteRepository : IRouteRepository
    {
        private readonly AppDbContext _db;

        public MySqlRouteRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<RouteEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Routes
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<RouteEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Routes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task AddAsync(RouteEntity route, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            _db.Routes.Add(route);
            await _db.SaveChangesAsync(ct);

            _db.OutboxEvents.Add(new OutboxEvent
            {
                AggregateType = "Route",
                AggregateId = route.Id,
                EventType = "Created",
                CreatedUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        public async Task UpdateAsync(RouteEntity route, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            _db.Routes.Update(route);
            await _db.SaveChangesAsync(ct);

            _db.OutboxEvents.Add(new OutboxEvent
            {
                AggregateType = "Route",
                AggregateId = route.Id,
                EventType = "Updated",
                CreatedUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            var entity = await _db.Routes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (entity == null)
                return false;

            _db.Routes.Remove(entity);
            await _db.SaveChangesAsync(ct);

            _db.OutboxEvents.Add(new OutboxEvent
            {
                AggregateType = "Route",
                AggregateId = id,
                EventType = "Deleted",
                CreatedUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return true;
        }
    }
}
