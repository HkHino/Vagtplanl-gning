using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Authentication;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : BaseAdminController
{
    protected readonly JwtHelper _jwtHelper;
    
    public TestController(AppDbContext _db, JwtHelper jwtHelper) : base(_db)
    {
        _jwtHelper = jwtHelper;
    }
    
    [HttpGet]
    [Route("test")]
    public async Task<IActionResult> Start()
    {
        // Should only return this text if logged in as Admin
        var result = new
        {
            message = "Hello Admin World"
        };
        return Ok(result);
    }

    [HttpGet]
    [Route("test/create")]
    [AllowAnonymous]
    public async Task<IActionResult> Create()
    {   
        
        var user = await _db.Employees.FindAsync(1);
        if (user == null)
        {
            const string errorMsg = "Employee not found";
            return BadRequest(errorMsg);
        }
        else
        {
            var token = _jwtHelper.GenerateToken(user);
            return Ok(token);
        }
        
    }
}