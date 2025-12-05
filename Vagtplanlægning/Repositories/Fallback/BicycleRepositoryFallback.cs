using Vagtplanlægning.Models;

public class BicycleRepositoryFallback : IBicycleRepository
{
    private readonly MySqlBicycleRepository _sql;
    private readonly MongoBicycleRepository _mongo;

    public BicycleRepositoryFallback(MySqlBicycleRepository sql, MongoBicycleRepository mongo)
    {
        _sql = sql;
        _mongo = mongo;
    }

    // --------- Helpers ---------

    private async Task<T> WithFallback<T>(
        Func<MySqlBicycleRepository, Task<T>> sqlAction,
        Func<MongoBicycleRepository, Task<T>> mongoAction)
    {
        try
        {
            return await sqlAction(_sql);
        }
        catch
        {
            return await mongoAction(_mongo);
        }
    }

    private async Task WithFallback(
        Func<MySqlBicycleRepository, Task> sqlAction,
        Func<MongoBicycleRepository, Task> mongoAction)
    {
        try
        {
            await sqlAction(_sql);
        }
        catch
        {
            await mongoAction(_mongo);
        }
    }

    // --------- IBicycleRepository-implementation ---------

    public Task<IEnumerable<Bicycle>> GetAllAsync(CancellationToken ct = default)
        => WithFallback(
            s => s.GetAllAsync(ct),
            m => m.GetAllAsync(ct));

    public Task<Bicycle?> GetByIdAsync(int id, CancellationToken ct = default)
        => WithFallback(
            s => s.GetByIdAsync(id, ct),
            m => m.GetByIdAsync(id, ct));

    public Task AddAsync(Bicycle bicycle, CancellationToken ct = default)
        => WithFallback(
            s => s.AddAsync(bicycle, ct),
            m => m.AddAsync(bicycle, ct));

    public Task UpdateAsync(Bicycle bicycle, CancellationToken ct = default)
        => WithFallback(
            s => s.UpdateAsync(bicycle, ct),
            m => m.UpdateAsync(bicycle, ct));

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => WithFallback(
            s => s.DeleteAsync(id, ct),
            m => m.DeleteAsync(id, ct));
}
