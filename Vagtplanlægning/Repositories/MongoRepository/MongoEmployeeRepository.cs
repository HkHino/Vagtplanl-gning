using MongoDB.Driver;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    /// <summary>
    /// MongoDB-based implementation of <see cref="IEmployeeRepository"/>.
    ///
    /// Used either as the primary store (when <c>DatabaseProvider = "mongo"</c>)
    /// or as a fallback store when MySQL is temporarily unavailable.
    /// </summary>
    public class MongoEmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;
        /// <summary>
        /// Creates a new Mongo-backed employee repository.
        /// </summary>
        /// <param name="database">Mongo database used for storing employees.</param>
        public MongoEmployeeRepository(IMongoDatabase database)
        {
            // Use a collection name that matches the MySQL table for sanity.
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
            // If EmployeeId is 0 (typical when it comes from the API),
            // generate a new "pseudo identity" by looking at the max EmployeeId in Mongo.
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

            // If we wanted to be extra defensive, we could check result.MatchedCount == 0 here.
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
