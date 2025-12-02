using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public interface IRouteRepository
    {
        Task<IEnumerable<RouteEntity>> GetAllAsync(CancellationToken ct = default);
        Task<RouteEntity?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(RouteEntity route, CancellationToken ct = default);
        Task UpdateAsync(RouteEntity route, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
