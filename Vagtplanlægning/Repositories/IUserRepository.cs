using Vagtplanlægning.Models;
using Vagtplanlægning.Utilities;

namespace Vagtplanlægning.Repositories
{
    public interface IUserRepository
    {
        Task<PagedResult<User>> GetAllAsync(int page, int pageSize, string? search, string? sort);
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        void Update(User user);
        void Remove(User user);
        Task<int> SaveChangesAsync();
    }
}
