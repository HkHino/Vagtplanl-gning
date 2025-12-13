using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Controllers.EmployeeController;
/// <summary>
/// API controller for the currently logged-in employee.
///
/// This controller is a thin layer on top of:
/// - <see cref="BaseEmployeeController"/> for access to <c>UserPrincipal</c>, DbContext and AutoMapper.
/// - <see cref="IUserRepository"/> for data access.
///
/// It exposes endpoints to:
/// - Read information about the logged-in user (<see cref="UserDto"/>).
/// - Read the employee's own shifts (<see cref="ShiftDto"/>).
/// - Read routes for a given employee (<see cref="RouteDto"/>).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : BaseEmployeeController
{
    /// <summary>
    /// Creates a new <see cref="EmployeeController"/>.
    /// </summary>
    /// <param name="db">The EF Core database context.</param>
    /// <param name="mapper">AutoMapper instance used to map entities to DTOs.</param>
    /// <param name="userRepository">Repository providing access to user-related data.</param>
    private readonly IUserRepository _userRepository;


    public EmployeeController(IMapper mapper, IUserRepository userRepository) : base(mapper)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Returns information about the currently logged-in user.
    /// </summary>
    /// <returns>
    /// HTTP 200 with a <see cref="UserDto"/> if the user exists,
    /// otherwise HTTP 404 if the user could not be found.
    /// </returns>

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
    /// <summary>
    /// Returns the list of shifts assigned to the currently logged-in employee.
    /// </summary>
    /// <returns>
    /// HTTP 200 with a collection of <see cref="ShiftDto"/> representing the employee's shifts.
    /// </returns>

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
    /// <summary>
    /// Returns the routes belonging to a specific employee.
    /// 
    /// This endpoint is anonymous and does not require authentication.
    /// </summary>
    /// <param name="employeeId">The ID of the employee whose routes should be returned.</param>
    /// <returns>
    /// HTTP 200 with a collection of <see cref="RouteDto"/> for the employee.
    /// </returns>

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
    
    [HttpDelete]
    [Route("{userId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(int userId)
    {
        var result = await _userRepository.DeleteAsync(userId);
        return Ok(result);
    }
}