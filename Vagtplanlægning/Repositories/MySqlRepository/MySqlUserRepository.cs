using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories;

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
}