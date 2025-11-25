using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    /// <summary>
    /// Fallback-repository:
    ///  - Prøver MySQL først
    ///  - Hvis der er DB/connection-fejl, falder tilbage til MongoDB
    /// </summary>
    public class EmployeeRepositoryFallback : IEmployeeRepository
    {
        private readonly MySqlEmployeeRepository _primary;
        private readonly MongoEmployeeRepository _secondary;
        private readonly ILogger<EmployeeRepositoryFallback> _logger;

        public EmployeeRepositoryFallback(
            MySqlEmployeeRepository primary,
            MongoEmployeeRepository secondary,
            ILogger<EmployeeRepositoryFallback> logger)
        {
            _primary = primary;
            _secondary = secondary;
            _logger = logger;
        }

        private static bool IsTransient(Exception ex)
        {
            // Du kan tweake denne, men det her er et godt startpunkt:
            return ex is MySqlException
                || ex is DbUpdateException
                || ex.InnerException is MySqlException;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                return await _primary.GetAllAsync(ct);
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                _logger.LogWarning(ex, "MySQL failed in GetAllAsync, falling back to MongoDB");
                return await _secondary.GetAllAsync(ct);
            }
        }

        public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                return await _primary.GetByIdAsync(id, ct);
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                _logger.LogWarning(ex, "MySQL failed in GetByIdAsync, falling back to MongoDB");
                return await _secondary.GetByIdAsync(id, ct);
            }
        }

        public async Task AddAsync(Employee employee, CancellationToken ct = default)
        {
            try
            {
                await _primary.AddAsync(employee, ct);
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                _logger.LogWarning(ex, "MySQL failed in AddAsync, falling back to MongoDB");
                await _secondary.AddAsync(employee, ct);
            }
        }

        public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
        {
            try
            {
                await _primary.UpdateAsync(employee, ct);
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                _logger.LogWarning(ex, "MySQL failed in UpdateAsync, falling back to MongoDB");
                await _secondary.UpdateAsync(employee, ct);
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            try
            {
                return await _primary.DeleteAsync(id, ct);
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                _logger.LogWarning(ex, "MySQL failed in DeleteAsync, falling back to MongoDB");
                return await _secondary.DeleteAsync(id, ct);
            }
        }
    }
}
