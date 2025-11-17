using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IShiftRepository _shiftRepo;

        public ShiftController(AppDbContext db, IShiftRepository shiftRepo)
        {
            _db = db;
            _shiftRepo = shiftRepo;
        }

      
        [HttpPut("{shiftId:int}/substitution-flag")]
        public async Task<IActionResult> MarkShiftSubstituted(int shiftId, [FromQuery] bool hasSubstituted)
        {
            await _shiftRepo.MarkShiftSubstitutedAsync(shiftId, hasSubstituted);
            return NoContent();
        }

        
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateShiftDto dto)
        {
            var date = dto.Day.Date;

            // Find or create DayEntity
            var day = await _db.Days
                .SingleOrDefaultAsync(d => d.Day == date);

            if (day == null)
            {
                day = new DayEntity { Day = date };
                _db.Days.Add(day);
                await _db.SaveChangesAsync();
            }

            // Handle substitutedId: null / 0 / real ID
            int? substitutedId = null;

            if (dto.SubstitutedId.HasValue && dto.SubstitutedId.Value != 0)
            {
                // Validate that the substitute exists
                bool exists = await _db.Substituteds
                    .AnyAsync(s => s.SubstitutedId == dto.SubstitutedId.Value);

                if (!exists)
                {
                    return BadRequest(new
                    {
                        error = $"Invalid substitutedId: {dto.SubstitutedId.Value}. No such substitute exists."
                    });
                }

                substitutedId = dto.SubstitutedId.Value;
            }

            var shift = new Shift
            {
                DayId = day.DayId,
                EmployeeId = dto.EmployeeId,
                BicycleId = dto.BicycleId,
                RouteNumberId = dto.RouteNumberId,
                MeetInTime = dto.MeetInTime,
                SubstitutedId = substitutedId
            };

            _db.Shifts.Add(shift);
            await _db.SaveChangesAsync();

            return NoContent();
        }

       
        [HttpPut("{shiftId:int}/start")]
        public async Task<IActionResult> SetStart(int shiftId, [FromQuery] TimeSpan startTime)
        {
            var shift = await _db.Shifts.FindAsync(shiftId);

            if (shift == null)
                return NotFound();

            shift.StartTime = startTime;

            await _db.SaveChangesAsync();
            return NoContent();
        }

       
        [HttpPut("{shiftId:int}/end")]
        public async Task<IActionResult> SetEnd(int shiftId, [FromQuery] TimeSpan endTime)
        {
            var shift = await _db.Shifts.FindAsync(shiftId);

            if (shift == null)
                return NotFound();

            shift.EndTime = endTime;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
