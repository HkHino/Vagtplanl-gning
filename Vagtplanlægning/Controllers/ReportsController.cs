using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;

    [HttpGet("monthly-hours")]
    public async Task<ActionResult<IEnumerable<MonthlyHoursRow>>> GetMonthlyHours(
        [FromQuery] int? employeeId, [FromQuery] int year, [FromQuery] int month)
    {
        object emp = employeeId.HasValue ? employeeId.Value : DBNull.Value;
        var rows = await _db.MonthlyHours
            .FromSqlRaw("CALL GetMonthlyHours({0},{1},{2})", emp, year, month)
            .ToListAsync();

        return Ok(rows);
    }
}
