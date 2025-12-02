using Microsoft.Extensions.Logging;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class RouteRepositoryFallback : IRouteRepository
    {
        private readonly MySqlRouteRepository _sql;
        private readonly MongoRouteRepository _mongo;
        private readonly ILogger<RouteRepositoryFallback> _logger;

        public RouteRepositoryFallback(
            MySqlRouteRepository sql,
            MongoRouteRepository mongo,
            ILogger<RouteRepositoryFallback> logger)
        {
            _sql = sql;
            _mongo = mongo;
            _logger = logger;
        }

        public async Task<IEnumerable<RouteEntity>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                return await _sql.GetAllAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MySQL unavailable for Routes, falling back to MongoDB (GetAll).");
                return await _mongo.GetAllAsync(ct);
            }
        }

        public async Task<RouteEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                return await _sql.GetByIdAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MySQL unavailable for Routes, falling back to MongoDB (GetById).");
                return await _mongo.GetByIdAsync(id, ct);
            }
        }

        public async Task AddAsync(RouteEntity route, CancellationToken ct = default)
        {
            try
            {
                await _sql.AddAsync(route, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MySQL unavailable for Routes, falling back to MongoDB (Add).");
                await _mongo.AddAsync(route, ct);
            }
        }

        public async Task UpdateAsync(RouteEntity route, CancellationToken ct = default)
        {
            try
            {
                await _sql.UpdateAsync(route, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MySQL unavailable for Routes, falling back to MongoDB (Update).");
                await _mongo.UpdateAsync(route, ct);
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            try
            {
                return await _sql.DeleteAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MySQL unavailable for Routes, falling back to MongoDB (Delete).");
                return await _mongo.DeleteAsync(id, ct);
            }
        }
    }
}
