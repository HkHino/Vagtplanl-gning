using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MySqlEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _db;

        public MySqlEmployeeRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Employees
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == id, ct);
        }

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
            var existing = await _db.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id, ct);

            if (existing == null)
                return false;

            _db.Employees.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
