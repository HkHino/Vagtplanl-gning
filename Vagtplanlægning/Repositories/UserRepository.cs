using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;
using Vagtplanlægning.Utilities;

namespace Vagtplanlægning.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<PagedResult<User>> GetAllAsync(int page, int pageSize, string? search, string? sort)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Name.Contains(search));

      

            var total = await query.CountAsync();
            var items = await query
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<User>(items, total, page, pageSize);
        }

        public Task<User?> GetByIdAsync(int id) =>
            _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

        public Task AddAsync(User user)
        {
            _db.Users.Add(user);
            return Task.CompletedTask;
        }

        public void Update(User user) => _db.Users.Update(user);
        public void Remove(User user) => _db.Users.Remove(user);
        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
