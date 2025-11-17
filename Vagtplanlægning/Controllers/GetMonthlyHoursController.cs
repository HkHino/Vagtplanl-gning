using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Data;
using Microsoft.EntityFrameworkCore;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GetMonthlyHoursController : ControllerBase
    {
        private readonly AppDbContext _db;
        public GetMonthlyHoursController(AppDbContext db) => _db = db;
     

    [HttpGet("monthly-hours")]

        public async Task<ActionResult<IEnumerable<MonthlyHoursRow>>> GetMonthlyHours(
    [FromQuery] int? employeeId,
    [FromQuery] int year,
    [FromQuery] int month)
        {
            // Payroll period: 26th of previous month → 25th of given month
            var periodEnd = new DateTime(year, month, 25);
            var periodStart = periodEnd.AddMonths(-1).AddDays(1); // 26th of previous month

            // Start from all employees (or a single one if filtered)
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

                    // === totalMonthlyHours: COALESCE(w.totalHours, computed SUM from shifts) ===
                    TotalMonthlyHours =
                        // Try to use WorkHoursInMonths first (if an aggregate row exists)
                        (
                            from w in _db.WorkHoursInMonths
                            where w.EmployeeId == e.EmployeeId
                               && w.PeriodStart == periodStart
                               && w.PeriodEnd == periodEnd
                            select (decimal?)w.TotalHours
                        ).FirstOrDefault()
                        ??
                        // Otherwise, compute from ListOfShift + Days + Substituteds
                        (
                            from s in _db.ListOfShift
                            join d in _db.Days on s.DayId equals d.DayId
                            join sub in _db.Substituteds on s.SubstitutedId equals sub.SubstitutedId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where d.Day >= periodStart && d.Day <= periodEnd
                                  // Same CASE logic as in the procedure:
                                  // - If substitutedId is not null and sub.employeeId == e.employeeId → use s.totalHours
                                  // - If substitutedId is null and s.employeeId == e.employeeId → use s.totalHours
                                  // - Else → 0
                                  && (
                                        (s.SubstitutedId != null && sub != null && sub.EmployeeId == e.EmployeeId)
                                     || (s.SubstitutedId == null && s.EmployeeId == e.EmployeeId)
                                     )
                            select (decimal?)s.TotalHours
                        ).Sum() ?? 0m,

                    // === hasSubstituted: COALESCE(w.hasSubstituted, computed flag) ===
                    HasSubstituted =
                        // Prefer WorkHoursInMonths.hasSubstituted if available
                        (
                            from w in _db.WorkHoursInMonths
                            where w.EmployeeId == e.EmployeeId
                               && w.PeriodStart == periodStart
                               && w.PeriodEnd == periodEnd
                            select (bool?)w.HasSubstituted
                        ).FirstOrDefault()
                        ??
                        // Otherwise compute: did this employee ever act as substitute in the period?
                        (
                            from s in _db.ListOfShift
                            join d in _db.Days on s.DayId equals d.DayId
                            join sub in _db.Substituteds on s.SubstitutedId equals sub.SubstitutedId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where d.Day >= periodStart && d.Day <= periodEnd
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
