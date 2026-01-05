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
        => await _db.Bicycles.AsNoTracking()
            .FirstOrDefaultAsync(b => b.BicycleId == id, ct);

    public async Task AddAsync(Bicycle bicycle, CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        _db.Bicycles.Add(bicycle);
        await _db.SaveChangesAsync(ct);

        _db.OutboxEvents.Add(new OutboxEvent
        {
            AggregateType = "Bicycle",
            AggregateId = bicycle.BicycleId,
            EventType = "Created",
            CreatedUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task UpdateAsync(Bicycle bicycle, CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        _db.Bicycles.Update(bicycle);
        await _db.SaveChangesAsync(ct);

        _db.OutboxEvents.Add(new OutboxEvent
        {
            AggregateType = "Bicycle",
            AggregateId = bicycle.BicycleId,
            EventType = "Updated",
            CreatedUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var entity = await _db.Bicycles.FindAsync(new object[] { id }, ct);
        if (entity == null)
            return false;

        _db.Bicycles.Remove(entity);
        await _db.SaveChangesAsync(ct);

        _db.OutboxEvents.Add(new OutboxEvent
        {
            AggregateType = "Bicycle",
            AggregateId = id,
            EventType = "Deleted",
            CreatedUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
