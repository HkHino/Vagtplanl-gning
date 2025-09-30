using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _db;
        public EmployeeRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Employee>> GetAllAsync() =>
            await _db.Employees.AsNoTracking().ToListAsync();

        public async Task<Employee?> GetByIdAsync(int id) =>
            await _db.Employees.FindAsync(id);

        public async Task AddAsync(Employee e)
        {
            _db.Employees.Add(e);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee e)
        {
            _db.Employees.Update(e);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Employees.FindAsync(id);
            if (entity == null) return false;
            _db.Employees.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
