using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Services;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IMonthlyHoursReportService _service;

        public ReportsController(IMonthlyHoursReportService service)
        {
            _service = service;
        }

        // GET: api/reports/monthly-hours?employeeId=2&year=2025&month=11
        [HttpGet("monthly-hours")]
        public async Task<ActionResult<IEnumerable<MonthlyHoursRow>>> GetMonthlyHours(
           [FromQuery] int? employeeId,
           [FromQuery] int year,
           [FromQuery] int month,
           CancellationToken ct = default)
        {
            if (year < 2020)
            {
                return BadRequest(new
                {
                    error = "Data is only available from January 2020 and onwards."
                });
            }

            if (month < 1 || month > 12)
            {
                return BadRequest(new
                {
                    error = "Month must be between 1 and 12."
                });
            }

            var rows = await _service.GetMonthlyHoursAsync(employeeId, year, month, ct);
            return Ok(rows);
        }
    }
}
