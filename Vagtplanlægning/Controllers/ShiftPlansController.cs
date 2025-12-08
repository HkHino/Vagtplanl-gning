using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftPlansController : ControllerBase
    {
        private readonly IShiftPlanRepository _planRepo;
        private readonly IShiftPlanService _planService;

        public ShiftPlansController(IShiftPlanRepository planRepo, IShiftPlanService planService)
        {
            _planRepo = planRepo;
            _planService = planService;
        }

        // --------------------------------------------------------------------
        // GET: api/shiftplans
        // Returnerer liste af planer (summary – uden shifts)
        // --------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftPlanSummaryDto>>> GetAll(
            CancellationToken ct)
        {
            var plans = await _planRepo.GetAllAsync(ct);

            var dtos = plans.Select(p => new ShiftPlanSummaryDto
            {
                ShiftPlanId = p.ShiftPlanId,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ShiftCount = p.Shifts?.Count ?? 0
            }).ToList();

            return Ok(dtos);
        }

        // --------------------------------------------------------------------
        // GET: api/shiftplans/{id}
        // Returnerer én plan inkl. alle shifts
        // --------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftPlanDetailDto>> GetById(
            string id, CancellationToken ct)
        {
            var plan = await _planRepo.GetByIdAsync(id, ct);
            if (plan == null)
            {
                return NotFound(new
                {
                    error = $"ShiftPlan with id '{id}' not found."
                });
            }

            var dto = ToDto(plan);
            return Ok(dto);
        }

        // --------------------------------------------------------------------
        // POST: api/shiftplans/generate-6weeks
        // Body: { "startDate": "2025-11-25T00:00:00" }
        // --------------------------------------------------------------------
        [HttpPost("generate-6weeks")]
        public async Task<ActionResult<ShiftPlanDetailDto>> Generate6Weeks(
            [FromBody] GenerateShiftPlanRequestDto request,
            CancellationToken ct)
        {
            if (request == null || request.StartDate == default)
            {
                return BadRequest(new
                {
                    error = "StartDate is required in the request body."
                });
            }

            var plan = await _planService.Generate6WeekPlanAsync(
                request.StartDate.Date,
                ct);

            var dto = ToDto(plan);

            return CreatedAtAction(
                nameof(GetById),
                new { id = dto.ShiftPlanId },
                dto);
        }

        // --------------------------------------------------------------------
        // DELETE: api/shiftplans/{id}
        // --------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            var deleted = await _planRepo.DeleteAsync(id, ct);
            if (!deleted)
            {
                return NotFound(new
                {
                    error = $"ShiftPlan with id '{id}' not found."
                });
            }

            return NoContent();
        }

        // --------------------------------------------------------------------
        // PUT: api/shiftplans/{id}/name
        // --------------------------------------------------------------------
        [HttpPut("{id}/name")]
        public async Task<IActionResult> UpdateName(
            string id,
            [FromBody] UpdateShiftPlanNameDto dto,
            CancellationToken ct = default)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { error = "Name is required." });
            }

            var plan = await _planRepo.GetByIdAsync(id, ct);
            if (plan == null)
            {
                return NotFound(new { error = $"ShiftPlan with id '{id}' not found." });
            }

            plan.Name = dto.Name.Trim();
            await _planRepo.UpdateAsync(plan, ct);

            return NoContent();
        }

        // --------------------------------------------------------------------
        // PUT: api/shiftplans/{id}/shifts/{index}
        // --------------------------------------------------------------------
        [HttpPut("{id}/shifts/{index:int}")]
        public async Task<ActionResult<ShiftPlanDetailDto>> UpdateShiftInPlan(
    string id,
    int index,
    [FromBody] UpdateShiftInPlanDto dto,
    CancellationToken ct = default)
        {
            if (dto == null)
            {
                return BadRequest(new { error = "Request body is missing or invalid." });
            }

            // 🔹 NYT: valider at datoen faktisk er sat
            if (dto.DateOfShift == default)
            {
                return BadRequest(new { error = "DateOfShift is required." });
            }

            var plan = await _planRepo.GetByIdAsync(id, ct);
            if (plan == null)
            {
                return NotFound(new { error = $"ShiftPlan with id '{id}' not found." });
            }

            if (plan.Shifts == null || index < 0 || index >= plan.Shifts.Count)
            {
                return BadRequest(new
                {
                    error = $"Shift index {index} is out of range.",
                    maxIndex = (plan.Shifts?.Count ?? 0) - 1
                });
            }

            var shift = plan.Shifts[index];

            shift.DateOfShift = dto.DateOfShift;
            shift.EmployeeId = dto.EmployeeId;
            shift.BicycleId = dto.BicycleId;
            shift.RouteId = dto.RouteId;
            shift.SubstitutedId = dto.SubstitutedId;

            await _planRepo.UpdateAsync(plan, ct);

            var detailDto = ToDto(plan);
            return Ok(detailDto);
        }


        // ====================================================================
        // Mapping helpers: Domain -> DTO
        // ====================================================================
        private static ShiftPlanDetailDto ToDto(ShiftPlan p)
        {
            return new ShiftPlanDetailDto
            {
                ShiftPlanId = p.ShiftPlanId,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Shifts = (p.Shifts ?? new List<Shift>())
                    .Select(s => new ShiftInPlanDto
                    {
                        ShiftId = s.ShiftId,
                        DateOfShift = s.DateOfShift,
                        EmployeeId = s.EmployeeId,
                        BicycleId = s.BicycleId,
                        SubstitutedId = s.SubstitutedId,
                        RouteId = s.RouteId,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        TotalHours = s.TotalHours
                    })
                    .ToList()
            };
        }
    }
}
