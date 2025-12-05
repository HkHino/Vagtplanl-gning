using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Authentication;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;
using Vagtplanlægning.Models.ApiModels;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Controllers.PublicControllers;

[ApiController]
[Route("[controller]")]
public class AuthController : BaseController
{
    private readonly JwtHelper _jwtHelper;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;


    public AuthController(AppDbContext db, IMapper mapper, JwtHelper jwtHelper, IUserRepository userRepository, IEmployeeRepository employeeRepository) : base(db, mapper)
    {
        _jwtHelper = jwtHelper;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    [HttpPost]
    [Route("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        // Check if any user with given username exists
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Checks if the passwords match between found user, and password in request
        if (!PasswordHelper.VerifyHash(request.Password, user.Hash))
        {
            return Unauthorized("Invalid password");
        }

        // Generates a JwT Token for the found user, if password matches
        var token = _jwtHelper.GenerateToken(user);

        return Ok(token);
    }

    [HttpPost]
    [Route("sign-up")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        // Checks for conflicts
        if (await _userRepository.UsernameInUse(request.Username))
        {
            return Conflict("Username is already in use");
        }

        if (await _employeeRepository.EmailInUse(request.Email))
        {
            return Conflict("Email is already in use");
        }

        // Create new employee and user
        var newEmployee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
        };
        await _employeeRepository.AddAsync(newEmployee);
        
        var hashedPassword = PasswordHelper.HashPassword(request.Password);
        var newUser = new User
        {
            Username = request.Username,
            Hash = hashedPassword,
            EmployeeId = newEmployee.EmployeeId
        };
        await _userRepository.AddAsync(newUser);
        
        return Ok("User successfully created");
    }
}