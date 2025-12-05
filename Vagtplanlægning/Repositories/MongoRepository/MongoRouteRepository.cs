using MongoDB.Driver;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

public class MongoRouteRepository : IRouteRepository
{
    private readonly IMongoCollection<RouteEntity> _collection;

    public MongoRouteRepository(IMongoDatabase db)
    {
        _collection = db.GetCollection<RouteEntity>("Routes");
    }

    public async Task<IEnumerable<RouteEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await _collection
            .Find(Builders<RouteEntity>.Filter.Empty)
            .ToListAsync(ct);
    }

    public async Task<RouteEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var filter = Builders<RouteEntity>.Filter.Eq(r => r.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(RouteEntity route, CancellationToken ct = default)
    {
        // Autoincrement på Id i Mongo
        if (route.Id == 0)
        {
            var last = await _collection
                .Find(Builders<RouteEntity>.Filter.Empty)
                .SortByDescending(r => r.Id)
                .Limit(1)
                .FirstOrDefaultAsync(ct);

            route.Id = (last?.Id ?? 0) + 1;
        }

        await _collection.InsertOneAsync(route, cancellationToken: ct);
    }

    public async Task UpdateAsync(RouteEntity route, CancellationToken ct = default)
    {
        var filter = Builders<RouteEntity>.Filter.Eq(r => r.Id, route.Id);
        await _collection.ReplaceOneAsync(filter, route, cancellationToken: ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var filter = Builders<RouteEntity>.Filter.Eq(r => r.Id, id);
        var result = await _collection.DeleteOneAsync(filter, ct);
        return result.DeletedCount > 0;
    }
}
