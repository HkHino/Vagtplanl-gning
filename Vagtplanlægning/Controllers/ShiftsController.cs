using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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

        // --------------------------------------------------------------------
        // 1) CREATE SHIFT
        // --------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateShiftDto dto)
        {
            // ---- Validation: employee ----
            var employeeExists = await _db.Employees
                .AnyAsync(e => e.EmployeeId == dto.EmployeeId);

            if (!employeeExists)
            {
                var validEmployeeIds = await _db.Employees
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                return BadRequest(new
                {
                    error = $"Invalid employeeId: {dto.EmployeeId}.",
                    validEmployeeIds
                });
            }

            // ---- Validation: bicycle ----
            var bicycleExists = await _db.Bicycles
                .AnyAsync(b => b.BicycleId == dto.BicycleId);

            if (!bicycleExists)
            {
                var validBicycleIds = await _db.Bicycles
                    .Select(b => b.BicycleId)
                    .ToListAsync();

                return BadRequest(new
                {
                    error = $"Invalid bicycleId: {dto.BicycleId}.",
                    validBicycleIds
                });
            }

            // ---- Validation: route ----
            var routeExists = await _db.Routes
                .AnyAsync(r => r.RouteNumberId == dto.RouteId);

            if (!routeExists)
            {
                var validRouteIds = await _db.Routes
                    .Select(r => r.RouteNumberId)
                    .ToListAsync();

                return BadRequest(new
                {
                    error = $"Invalid routeId: {dto.RouteId}.",
                    validRouteIds
                });
            }

            // ---- Validation: substituted ----
            var substitutedExists = await _db.Substituteds
                .AnyAsync(s => s.SubstitutedId == dto.SubstitutedId);

            if (!substitutedExists)
            {
                var validSubstitutedIds = await _db.Substituteds
                    .Select(s => s.SubstitutedId)
                    .ToListAsync();

                return BadRequest(new
                {
                    error = $"Invalid substitutedId: {dto.SubstitutedId}.",
                    validSubstitutedIds
                });
            }

            var shift = new Shift
            {
                DateOfShift = dto.DateOfShift,
                EmployeeId = dto.EmployeeId,
                BicycleId = dto.BicycleId,
                RouteId = dto.RouteId,
                SubstitutedId = dto.SubstitutedId
            };

            try
            {
                _db.ListOfShift.Add(shift);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to save shift due to a database constraint.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }

            return NoContent();
        }

        // --------------------------------------------------------------------
        // 2) START SHIFT (flexible time parsing)
        // --------------------------------------------------------------------
        [HttpPut("{shiftId:int}/start")]
        public async Task<IActionResult> SetStart(int shiftId, [FromQuery] TimeSpan startTime)
        {
            var shift = await _db.ListOfShift.FindAsync(shiftId);

            if (shift == null)
                return NotFound(new { error = $"Shift with id {shiftId} not found." });

            shift.StartTime = startTime;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    error = "Could not save start time. The computed total hours would be out of allowed range.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }

            return NoContent();
        }

        [HttpPut("{shiftId:int}/end")]
        public async Task<IActionResult> SetEnd(int shiftId, [FromQuery] TimeSpan endTime)
        {
            var shift = await _db.ListOfShift.FindAsync(shiftId);

            if (shift == null)
                return NotFound(new { error = $"Shift with id {shiftId} not found." });

            shift.EndTime = endTime;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    error = "Could not save end time. The computed total hours would be out of allowed range.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }

            return NoContent();
        }


        // --------------------------------------------------------------------
        // 4) MARK SHIFT AS SUBSTITUTED (hasSubstituted API)
        // --------------------------------------------------------------------
        [HttpPut("{shiftId:int}/substitution-flag")]
        public async Task<IActionResult> MarkShiftSubstituted(int shiftId, [FromQuery] bool hasSubstituted)
        {
            await _shiftRepo.MarkShiftSubstitutedAsync(shiftId, hasSubstituted);
            return NoContent();
        }
               
    }
}
