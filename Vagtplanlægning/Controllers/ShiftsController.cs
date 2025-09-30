using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ShiftsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Shift>>> GetAll() =>
        Ok(await _db.ListOfShift.AsNoTracking().ToListAsync());

    // AddShift(pDay DATE, pEmployeeId, pBicycleId, pRouteNumberId, pMeetInTime, pSubstitutedId)
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateShiftDto dto)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "CALL AddShift({0},{1},{2},{3},{4},{5})",
            dto.Day.Date, dto.EmployeeId, dto.BicycleId, dto.RouteNumberId, dto.MeetInTime, dto.SubstitutedId);
        return NoContent();
    }

    [HttpPut("{shiftId:int}/start")]
    public async Task<IActionResult> SetStart(int shiftId, [FromQuery] TimeSpan startTime)
    {
        await _db.Database.ExecuteSqlRawAsync("CALL SetStartTime({0},{1})", shiftId, startTime);
        return NoContent();
    }

    [HttpPut("{shiftId:int}/end")]
    public async Task<IActionResult> SetEnd(int shiftId, [FromQuery] TimeSpan endTime)
    {
        await _db.Database.ExecuteSqlRawAsync("CALL SetEndTime({0},{1})", shiftId, endTime);
        return NoContent();
    }
}
