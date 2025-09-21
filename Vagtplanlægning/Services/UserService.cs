using AutoMapper;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Utilities;

namespace Vagtplanlægning.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PagedResult<UserDto>> GetUsersAsync(int page, int pageSize, string? search, string? sort)
        {
            var paged = await _repo.GetAllAsync(page, pageSize, search, sort);
            var dtos = paged.Items.Select(u => _mapper.Map<UserDto>(u));
            return new PagedResult<UserDto>(dtos, paged.TotalCount, paged.Page, paged.PageSize);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateAsync(int id, UpdateUserDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            _mapper.Map(dto, existing);
            _repo.Update(existing);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            _repo.Remove(existing);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}
