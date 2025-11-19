using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MySqlEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _db;
        public MySqlEmployeeRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default) =>
            await _db.Employees.AsNoTracking().ToListAsync(ct);

        public Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.Employees.FindAsync(new object?[] { id }, ct).AsTask();

        public async Task AddAsync(Employee employee, CancellationToken ct = default)
        {
            _db.Employees.Add(employee);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
        {
            _db.Employees.Update(employee);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.Employees.FindAsync(new object?[] { id }, ct);
            if (entity == null) return false;
            _db.Employees.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

