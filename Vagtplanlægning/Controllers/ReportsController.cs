using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;
using Vagtplanlægning.DTOs;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReportsController(AppDbContext db) => _db = db;

        [HttpGet("monthly-hours")]
        public async Task<ActionResult<IEnumerable<MonthlyHoursRow>>> GetMonthlyHours(
           [FromQuery] int? employeeId,
           [FromQuery] int year,
           [FromQuery] int month)
        {
            // ==== VALIDATION (Fix for invalid DateTime) ====
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

            var periodEnd = new DateTime(year, month, 25);
            var periodStart = periodEnd.AddMonths(-1).AddDays(1); // 26th of previous month

            var employees = _db.Employees.AsQueryable();
            if (employeeId.HasValue)
            {
                employees = employees.Where(e => e.EmployeeId == employeeId.Value);
            }

            var query =
                from e in employees
                select new MonthlyHoursRow
                {
                    EmployeeId = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Year = year,
                    Month = month,

                    TotalMonthlyHours =
                        // Try WorkHoursInMonths first
                        (
                            from w in _db.WorkHoursInMonths
                            where w.EmployeeId == e.EmployeeId
                                  && w.PeriodStart == periodStart
                                  && w.PeriodEnd == periodEnd
                            select (decimal?)w.TotalHours
                        ).FirstOrDefault()
                        ??
                        // Otherwise compute from shifts
                        (
                            from s in _db.ListOfShift
                            join sub in _db.Substituteds on s.SubstitutedId equals sub.SubstitutedId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where s.DateOfShift >= periodStart && s.DateOfShift <= periodEnd
                                  && (
                                          (s.SubstitutedId != null && sub != null && sub.EmployeeId == e.EmployeeId)
                                       || (s.SubstitutedId == null && s.EmployeeId == e.EmployeeId)
                                     )
                            select (decimal?)s.TotalHours
                        ).Sum() ?? 0m,

                    HasSubstituted =
                        (
                            from w in _db.WorkHoursInMonths
                            where w.EmployeeId == e.EmployeeId
                                  && w.PeriodStart == periodStart
                                  && w.PeriodEnd == periodEnd
                            select (bool?)w.HasSubstituted
                        ).FirstOrDefault()
                        ??
                        (
                            from s in _db.ListOfShift
                            join sub in _db.Substituteds on s.SubstitutedId equals sub.SubstitutedId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where s.DateOfShift >= periodStart && s.DateOfShift <= periodEnd
                                  && s.SubstitutedId != null
                                  && sub != null
                                  && sub.EmployeeId == e.EmployeeId
                            select 1
                        ).Any()
                };

            var rows = await query.ToListAsync();
            return Ok(rows);
        }
    }
}
