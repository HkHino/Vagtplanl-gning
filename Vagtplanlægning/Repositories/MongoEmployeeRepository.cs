using MongoDB.Driver;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MongoEmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _collection;

        public MongoEmployeeRepository(IMongoDatabase database)
        {
            // collection name can be whatever you want
            _collection = database.GetCollection<Employee>("employees");
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        {
            var cursor = await _collection.FindAsync(FilterDefinition<Employee>.Empty, cancellationToken: ct);
            return await cursor.ToListAsync(ct);
        }

        public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var cursor = await _collection.FindAsync(x => x.EmployeeId == id, cancellationToken: ct);
            return await cursor.FirstOrDefaultAsync(ct);
        }

        public Task AddAsync(Employee employee, CancellationToken ct = default) =>
            _collection.InsertOneAsync(employee, cancellationToken: ct);

        public Task UpdateAsync(Employee employee, CancellationToken ct = default) =>
            _collection.ReplaceOneAsync(x => x.EmployeeId == employee.EmployeeId, employee, cancellationToken: ct);

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var result = await _collection.DeleteOneAsync(x => x.EmployeeId == id, ct);
            return result.DeletedCount > 0;
        }
    }
}
