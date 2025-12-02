using Vagtplanlægning.Models;

public interface IBicycleRepository
{
    Task<IEnumerable<Bicycle>> GetAllAsync(CancellationToken ct = default);
    Task<Bicycle?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Bicycle bicycle, CancellationToken ct = default);
    Task UpdateAsync(Bicycle bicycle, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
