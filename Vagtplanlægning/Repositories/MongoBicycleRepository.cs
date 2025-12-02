using MongoDB.Driver;
using Vagtplanlægning.Models;

public class MongoBicycleRepository : IBicycleRepository
{
    private readonly IMongoCollection<Bicycle> _collection;

    public MongoBicycleRepository(IMongoDatabase db)
    {
        _collection = db.GetCollection<Bicycle>("Bicycles");
    }

    public async Task<IEnumerable<Bicycle>> GetAllAsync(CancellationToken ct = default)
        => await _collection.Find(_ => true).ToListAsync(ct);

    public async Task<Bicycle?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _collection.Find(b => b.BicycleId == id).FirstOrDefaultAsync(ct);

    public async Task AddAsync(Bicycle bicycle, CancellationToken ct = default)
    {
        if (bicycle == null) throw new ArgumentNullException(nameof(bicycle));

        // Hvis BicycleId ikke er sat (0), så find næste ledige ID
        if (bicycle.BicycleId == 0)
        {
            var last = await _collection
                .Find(Builders<Bicycle>.Filter.Empty)
                .SortByDescending(b => b.BicycleId)
                .Limit(1)
                .FirstOrDefaultAsync(ct);

            var nextId = (last?.BicycleId ?? 0) + 1;
            bicycle.BicycleId = nextId;
        }

        await _collection.InsertOneAsync(bicycle, cancellationToken: ct);
    }

    public async Task UpdateAsync(Bicycle bicycle, CancellationToken ct = default)
    {
        var result = await _collection.ReplaceOneAsync(
            b => b.BicycleId == bicycle.BicycleId,
            bicycle,
            cancellationToken: ct
        );
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var result = await _collection.DeleteOneAsync(b => b.BicycleId == id, ct);
        return result.DeletedCount > 0;
    }
}
