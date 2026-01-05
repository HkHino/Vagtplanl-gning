using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;
using Vagtplanlægning.Services;

namespace Vagtplanlægning.Repositories
{
    public class MySqlEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _db;
        private readonly OutboxWriter _outboxWriter;

        public MySqlEmployeeRepository(AppDbContext db, OutboxWriter outboxWriter)
        {
            _db = db;
            _outboxWriter = outboxWriter;
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
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                _db.Employees.Add(employee);
                await _db.SaveChangesAsync(ct);

                _outboxWriter.AddEvent(
                    aggregateType: "Employee",
                    aggregateId: employee.EmployeeId,
                    eventType: "Created"
                );

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                _db.Employees.Update(employee);
                await _db.SaveChangesAsync(ct);

                _outboxWriter.AddEvent(
                    aggregateType: "Employee",
                    aggregateId: employee.EmployeeId,
                    eventType: "Updated"
                );

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var existing = await _db.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeId == id, ct);

                if (existing == null)
                    return false;

                _db.Employees.Remove(existing);
                await _db.SaveChangesAsync(ct);

                _outboxWriter.AddEvent(
                    aggregateType: "Employee",
                    aggregateId: id,
                    eventType: "Deleted"
                );

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return true;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<bool> EmailInUse(string email, CancellationToken ct = default)
        {
            return await _db.Employees
                .AnyAsync(e => e.Email == email, ct);
        }
    }
}
