using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee e);
        Task UpdateAsync(Employee e);
        Task<bool> DeleteAsync(int id);
    }
}
