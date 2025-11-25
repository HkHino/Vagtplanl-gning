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
            return await _db.Routes.AsNoTracking().ToListAsync(ct);
        }

        public async Task<RouteEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // id = Routes.id (PK)
            return await _db.Routes.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }


        public async Task AddAsync(RouteEntity route, CancellationToken ct = default)
        {
            _db.Routes.Add(route);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.Routes.FindAsync(new object?[] { id }, ct);
            if (entity == null) return false;
            _db.Routes.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
