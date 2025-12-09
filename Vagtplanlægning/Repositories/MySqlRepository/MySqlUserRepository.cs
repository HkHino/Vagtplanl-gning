using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories.MySqlRepository;

public class MySqlUserRepository : IUserRepository
{
    
    private readonly AppDbContext _db;

    public MySqlUserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        var result = await _db.Users
            .SingleOrDefaultAsync(x => x.UserId == id);
        return result;
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _db.Users.SingleOrDefaultAsync(x => x.Username == username);
    }

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    // Checks if username is in use
    public async Task<bool> UsernameInUse(string username)
    {
        return await _db.Users.AnyAsync(x => x.Username == username);
    }

    public async Task<User?> GetByIdWithEmployeesAsync(int id)
    {
        return await _db.Users
            .Include(u => u.Employee)
            .SingleOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<Shift[]> GetShiftsAsync(int userId)
    {
        // Get the employee Id of the current user
        var user = await _db.Users
            .Include(u => u.Employee)
            .SingleOrDefaultAsync(u => u.UserId == userId);
        
        // All shifts which user has
        var shifts = await _db.ListOfShift
            .Where(s => s.EmployeeId == user.UserId)
            .ToArrayAsync();
        return shifts;
    }

    public async Task<RouteEntity[]> GetRoutesByEmployeeIdAsync(int employeeId)
    {
        var user = await _db.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.Shifts)
            .ThenInclude((s => s.Routes))
            .FirstOrDefaultAsync(i => i.EmployeeId == employeeId);
        if (user == null) return null;
        
        // Make an empty array to store routes in
        var routesList = new List<RouteEntity>();
        
        // Go through all shifts and find routes
        foreach (var shift in user.Employee.Shifts)
        {
            if (shift.Routes != null && !routesList.Contains(shift.Routes))
            {
                routesList.Add(shift.Routes);
            }
        }
        return routesList.ToArray();
    }
}