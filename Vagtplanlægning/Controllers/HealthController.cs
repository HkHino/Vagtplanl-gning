using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public HealthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpGet]
    public IActionResult Get() => Ok(new { status = "API is running" });

    [HttpGet("db")]
    public async Task<IActionResult> GetDbHealth()
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1");
            return Ok(new { status = "Database connection OK" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "Database connection FAILED", error = ex.Message });
        }
    }

    [HttpGet("employee/{id:int}")]
    public async Task<IActionResult> CheckEmployee(int id)
    {
        var e = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == id);
        if (e == null) return NotFound(new { status = "Employee NOT found", id });
        return Ok(new { status = "Employee found", id = e.EmployeeId, name = $"{e.FirstName} {e.LastName}".Trim(), email = e.Email });
    }

    [HttpGet("info")]
    public IActionResult Info()
    {
        var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
        string dbName = "unknown";
        foreach (var part in connStr.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0].Trim().Equals("Database", StringComparison.OrdinalIgnoreCase))
                dbName = kv[1].Trim();
        }

        return Ok(new
        {
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            database = dbName,
            provider = _db.Database.ProviderName,
            connected = _db.Database.CanConnect()
        });
    }
    [HttpGet("config")]
    public IActionResult GetConfig([FromServices] IConfiguration config)
    {
        return Ok(new
        {
            provider = config["DatabaseProvider"]
        });
    }

}
