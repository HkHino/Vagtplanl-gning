using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    
    // Checks if username is already used
    Task<bool> UsernameInUse(string username);
    // Get By ID, and includes the employee
    Task<User?> GetByIdWithEmployeesAsync(int id);
    
    // Get an employee's shifts based on the Id
    Task<Shift[]> GetShiftsAsync(int userId);
}