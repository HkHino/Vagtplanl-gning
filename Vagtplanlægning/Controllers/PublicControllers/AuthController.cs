using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Authentication;
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
    private readonly ILogger<EmployeeRepositoryFallback> _logger;


    public AuthController(IMapper mapper, 
        JwtHelper jwtHelper, 
        IUserRepository userRepository, 
        IEmployeeRepository employeeRepository, 
        ILogger<EmployeeRepositoryFallback> logger
        ) : base(mapper)
    {
        _jwtHelper = jwtHelper;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    [HttpPost]
    [Route("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest? request)
    {
        if (request == null) return BadRequest("Invalid request");

        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null) return NotFound("User not found");

            if (!PasswordHelper.VerifyHash(request.Password, user.Hash))
                return Unauthorized("Invalid password");

            var token = _jwtHelper.GenerateToken(user);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,              // dev http => false, prod https => true
                Expires = DateTimeOffset.UtcNow.AddDays(1),
                Path = "/"
            });

            // return role so frontend can route immediately without reading JWT
            return Ok(new { role = user.Role.ToString().ToLowerInvariant() });
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed login attempt for {request} error: {Message}", request, e.Message);
            return NotFound("Not found.");
        }
    }


    [HttpPost]
    [Route("sign-out")]
    public IActionResult SignOut()
    {
        Response.Cookies.Delete("access_token", new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps
        });

        return Ok();
    }



    [HttpGet]
    [Route("me")]
    [Authorize]
    public IActionResult Me()
    {
        var u = JwtHelper.GetUser(User);
        return Ok(new { id = u.Id, username = u.Username, role = u.Role.ToLowerInvariant() });
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