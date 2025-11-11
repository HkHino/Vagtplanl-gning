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
            return await _db.Route.AsNoTracking().ToListAsync(ct);
        }

        public async Task<RouteEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Route.AsNoTracking().FirstOrDefaultAsync(r => r.RouteNumberId == id, ct);
        }

        public async Task AddAsync(RouteEntity route, CancellationToken ct = default)
        {
            _db.Route.Add(route);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.Route.FindAsync(new object?[] { id }, ct);
            if (entity == null) return false;
            _db.Route.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
