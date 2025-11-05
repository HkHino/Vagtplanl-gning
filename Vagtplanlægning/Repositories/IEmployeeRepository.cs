using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default);
        Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(Employee employee, CancellationToken ct = default);
        Task UpdateAsync(Employee employee, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}