using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    /// <summary>
    /// Abstraction for employee data access.
    ///
    /// Implementations can be backed by different data stores (MySQL, MongoDB, Neo4j, etc.),
    /// which allows the rest of the system (controllers/services) to stay agnostic to the
    /// underlying storage.
    /// </summary>
    public interface IEmployeeRepository
    {  /// <summary>
       /// Returns all employees.
       /// </summary>
        Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default);
        /// <summary>
        /// Returns a single employee by id, or <c>null</c> if not found.
        /// </summary>
        Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default);
        /// <summary>
        /// Persists a new employee.
        /// </summary>
        Task AddAsync(Employee employee, CancellationToken ct = default);
        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        Task UpdateAsync(Employee employee, CancellationToken ct = default);
        /// <summary>
        /// Deletes an employee by id.
        /// </summary>
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        /// <summary>
        /// Checks whether the given email address is already in use by another employee.
        /// </summary>        
        Task<bool> EmailInUse(string email, CancellationToken ct = default);
    }
}