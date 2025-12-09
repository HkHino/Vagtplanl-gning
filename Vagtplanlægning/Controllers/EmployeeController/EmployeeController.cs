using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet]
    [Route("get-employee-shifts")]
    public async Task<IActionResult> GetEmployeeShifts()
    {
        // Get list of shifts
        var shifts = await _userRepository.GetShiftsAsync(UserPrincipal.Id);
        
        // Convert the shifts into a DTO
        var response = _mapper.Map<IEnumerable<ShiftDto>>(shifts);
        return Ok(response);
    }
    
    [HttpGet]
    [Route("get-employee-routes-by-id/{employeeId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEmployeeRoutesById(int employeeId)
    {
        // Get list of routes
        var routes = await _userRepository.GetRoutesByEmployeeIdAsync(employeeId);
        Console.WriteLine("Routes:");
        foreach (var route in routes)
        {
            Console.WriteLine($"- {route.RouteNumber}");
        }
        // Convert the routes into a DTO
        var response = _mapper.Map<IEnumerable<RouteDto>>(routes);
        return Ok(response);
    }
}