using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

public class MySqlBicycleRepository : IBicycleRepository
{
    private readonly AppDbContext _db;

    public MySqlBicycleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Bicycle>> GetAllAsync(CancellationToken ct = default)
        => await _db.Bicycles.AsNoTracking().ToListAsync(ct);

    public async Task<Bicycle?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Bicycles.AsNoTracking().FirstOrDefaultAsync(b => b.BicycleId == id, ct);

    public async Task AddAsync(Bicycle bicycle, CancellationToken ct = default)
    {
        _db.Bicycles.Add(bicycle);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Bicycle bicycle, CancellationToken ct = default)
    {
        _db.Bicycles.Update(bicycle);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Bicycles.FindAsync(new object[] { id }, ct);
        if (e == null) return false;

        _db.Bicycles.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
