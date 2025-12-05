using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Controllers.EmployeeController;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : BaseEmployeeController
{
    private readonly IUserRepository _userRepository;


    public EmployeeController(AppDbContext db, IMapper mapper, IUserRepository userRepository) : base(db, mapper)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    [Route("get-user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        var currentUser = await _userRepository.GetByIdWithEmployeesAsync(UserPrincipal.Id);
        if (currentUser == null) return NotFound("User not found");
        
        // Converts into DTO
        var response = _mapper.Map<UserDto>(currentUser);
        return Ok(response);
    }
}