using MongoDB.Driver;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MongoEmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;

        public MongoEmployeeRepository(IMongoDatabase database)
        {
            // Brug en collection med samme navn som tabellen i MySQL for sanity
            _employees = database.GetCollection<Employee>("Employees");
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        {
            var cursor = await _employees
                .FindAsync(Builders<Employee>.Filter.Empty, cancellationToken: ct);

            return await cursor.ToListAsync(ct);
        }

        public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.EmployeeId, id);
            var cursor = await _employees.FindAsync(filter, cancellationToken: ct);
            return await cursor.FirstOrDefaultAsync(ct);
        }

        public async Task AddAsync(Employee employee, CancellationToken ct = default)
        {
            // Hvis EmployeeId er 0 (typisk når den kommer fra API’et),
            // så genererer vi et nyt "pseudo identity" ved at kigge på max EmployeeId i Mongo.
            if (employee.EmployeeId == 0)
            {
                var sort = Builders<Employee>.Sort.Descending(e => e.EmployeeId);
                var cursor = await _employees.FindAsync(Builders<Employee>.Filter.Empty,
                    new FindOptions<Employee>
                    {
                        Sort = sort,
                        Limit = 1
                    },
                    ct);

                var last = await cursor.FirstOrDefaultAsync(ct);
                var nextId = (last?.EmployeeId ?? 0) + 1;
                employee.EmployeeId = nextId;
            }

            await _employees.InsertOneAsync(employee, cancellationToken: ct);
        }

        public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.EmployeeId, employee.EmployeeId);

            var result = await _employees.ReplaceOneAsync(
                filter,
                employee,
                new ReplaceOptions { IsUpsert = false },
                ct);

            // Hvis vi ville være ekstra defensive, kunne vi tjekke result.MatchedCount == 0 her.
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.EmployeeId, id);
            var result = await _employees.DeleteOneAsync(filter, ct);
            return result.DeletedCount > 0;
        }

        // Checks if email is in use
        public async Task<bool> EmailInUse(string email, CancellationToken ct = default)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Email, email);
            return await _employees.Find(filter).Limit(1).AnyAsync(ct);
        }
    }
}
