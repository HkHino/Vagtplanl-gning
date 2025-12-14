using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;
using Vagtplanlægning.Models.DtoModels;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly IShiftRepository _shiftRepo;
        private readonly IShiftExecutionService _shiftExec;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IBicycleRepository _bicycleRepo;
        private readonly IRouteRepository _routeRepo;
        private readonly IMapper _mapper;

        public ShiftController(
            IShiftRepository shiftRepo,
            IShiftExecutionService shiftExec,
            IEmployeeRepository employeeRepo,
            IBicycleRepository bicycleRepo,
            IRouteRepository routeRepo, IMapper mapper)
        {
            _shiftRepo = shiftRepo;
            _shiftExec = shiftExec;
            _employeeRepo = employeeRepo;
            _bicycleRepo = bicycleRepo;
            _routeRepo = routeRepo;
            _mapper = mapper;
        }

        // --------------------------------------------------------------------
        // 1) CREATE SHIFT - nu via repositories (med fallback) i stedet for DbContext
        // --------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateShiftDto dto)
        {
            // ---------- Validation: employee ----------
            var employees = (await _employeeRepo.GetAllAsync()).ToList();
            var employeeExists = employees.Any(e => e.EmployeeId == dto.EmployeeId);

            if (!employeeExists)
            {
                var validEmployeeIds = employees.Select(e => e.EmployeeId).ToList();
                return BadRequest(new
                {
                    error = $"Invalid employeeId: {dto.EmployeeId}.",
                    validEmployeeIds
                });
            }

            // ---------- Validation: bicycle ----------
            var bicycles = (await _bicycleRepo.GetAllAsync()).ToList();
            var bicycleExists = bicycles.Any(b => b.BicycleId == dto.BicycleId);

            if (!bicycleExists)
            {
                var validBicycleIds = bicycles.Select(b => b.BicycleId).ToList();
                return BadRequest(new
                {
                    error = $"Invalid bicycleId: {dto.BicycleId}.",
                    validBicycleIds
                });
            }

            // ---------- Validation: route ----------
            var routes = (await _routeRepo.GetAllAsync()).ToList();
            var routeExists = routes.Any(r => r.Id == dto.RouteId);

            if (!routeExists)
            {
                var validRouteIds = routes.Select(r => r.Id).ToList();
                return BadRequest(new
                {
                    error = $"Invalid routeId: {dto.RouteId}.",
                    validRouteIds
                });
            }

            // ---------- SubstitutedId håndtering ----------
            
            // - Hvis klient ikke sender noget (0 eller negativt) → bind til employeeId
            // - Hvis klient sender et positivt id → brug det som det er
            int effectiveSubstitutedId =
                dto.SubstitutedId > 0
                    ? dto.SubstitutedId
                    : dto.EmployeeId;

            // ---------- Selve oprettelsen ----------
            try
            {
                // Her bruger vi _shiftRepo (som er FallbackShiftRepository),
                // så hvis MySQL er nede, prøver den Mongo som fallback.
                var shift = new Models.Shift
                {
                    DateOfShift = dto.DateOfShift,
                    EmployeeId = dto.EmployeeId,
                    BicycleId = dto.BicycleId,
                    RouteId = dto.RouteId,
                    SubstitutedId = effectiveSubstitutedId,
                    StartTime = null,
                    EndTime = null,
                    TotalHours = null
                };

                await _shiftRepo.AddAsync(shift);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to save shift via primary/fallback repositories.",
                    detail = ex.Message
                });
            }

            return NoContent();
        }

        // --------------------------------------------------------------------
        // 2) START SHIFT – via repo + service (med fallback)
        // --------------------------------------------------------------------
        [HttpPut("{shiftId:int}/start")]
        public async Task<IActionResult> SetStart(int shiftId, [FromQuery] TimeSpan startTime)
        {
            // Tjek først om shift findes via repository (som selv klarer fallback)
            var existing = await _shiftRepo.GetByIdAsync(shiftId);
            if (existing == null)
            {
                return NotFound(new { error = $"Shift with id {shiftId} not found." });
            }

            try
            {
                await _shiftExec.SetStartTimeAsync(shiftId, startTime);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Unexpected error while setting start time.",
                    detail = ex.Message
                });
            }
        }

        // --------------------------------------------------------------------
        // 3) END SHIFT – samme mønster
        // --------------------------------------------------------------------
        [HttpPut("{shiftId:int}/end")]
        public async Task<IActionResult> SetEnd(int shiftId, [FromQuery] TimeSpan endTime)
        {
            var existing = await _shiftRepo.GetByIdAsync(shiftId);
            if (existing == null)
            {
                return NotFound(new { error = $"Shift with id {shiftId} not found." });
            }

            try
            {
                await _shiftExec.SetEndTimeAsync(shiftId, endTime);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Unexpected error while setting end time.",
                    detail = ex.Message
                });
            }
        }

        // --------------------------------------------------------------------
        // 4) MARK SHIFT AS SUBSTITUTED – bruger repo med fallback
        // --------------------------------------------------------------------
        [HttpPut("{shiftId:int}/substitution-flag")]
        public async Task<IActionResult> MarkShiftSubstituted(int shiftId, [FromQuery] bool hasSubstituted)
        {
            await _shiftRepo.MarkShiftSubstitutedAsync(shiftId, hasSubstituted);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _shiftRepo.GetAllAsync();
            
            // Map
            var response = _mapper.Map<IEnumerable<ShiftDto>>(result);
            
            return Ok(response);
        }


        [HttpPut("{shiftId:int}")]
        public async Task<IActionResult> Update(int shiftId, [FromBody] UpdateShiftDto dto)
        {
            // 1) Find shift
            var existing = await _shiftRepo.GetByIdAsync(shiftId);
            if (existing == null)
            {
                return NotFound(new { error = $"Shift with id {shiftId} not found." });
            }

            // 2) Validation (samme som Create)
            var employees = (await _employeeRepo.GetAllAsync()).ToList();
            if (!employees.Any(e => e.EmployeeId == dto.EmployeeId))
            {
                return BadRequest(new { error = $"Invalid employeeId: {dto.EmployeeId}." });
            }

            var bicycles = (await _bicycleRepo.GetAllAsync()).ToList();
            if (!bicycles.Any(b => b.BicycleId == dto.BicycleId))
            {
                return BadRequest(new { error = $"Invalid bicycleId: {dto.BicycleId}." });
            }

            var routes = (await _routeRepo.GetAllAsync()).ToList();
            if (!routes.Any(r => r.Id == dto.RouteId))
            {
                return BadRequest(new { error = $"Invalid routeId: {dto.RouteId}." });
            }

            // 3) substituted fallback
            var effectiveSubstitutedId = dto.SubstitutedId > 0 ? dto.SubstitutedId : dto.EmployeeId;

            // 4) Update fields
            existing.DateOfShift = dto.DateOfShift;
            existing.EmployeeId = dto.EmployeeId;
            existing.BicycleId = dto.BicycleId;
            existing.RouteId = dto.RouteId;
            existing.SubstitutedId = effectiveSubstitutedId;

            // 5) Persist
            await _shiftRepo.UpdateAsync(existing); // skal eksistere i repo
            return NoContent();
        }



        [HttpDelete("{shiftId:int}")]
        public async Task<IActionResult> Delete(int shiftId)
        {
            var result = await _shiftRepo.DeleteAsync(shiftId);
            return Ok(result);
        }
    }
}
