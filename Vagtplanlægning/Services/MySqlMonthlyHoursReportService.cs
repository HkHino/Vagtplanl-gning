using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;

namespace Vagtplanlægning.Services
{
    public class MySqlMonthlyHoursReportService : IMonthlyHoursReportService
    {
        private readonly AppDbContext _db;

        public MySqlMonthlyHoursReportService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<MonthlyHoursRow>> GetMonthlyHoursAsync(
            int? employeeId,
            int year,
            int month,
            CancellationToken ct = default)
        {
            var periodEnd = new DateTime(year, month, 25);
            var periodStart = periodEnd.AddMonths(-1).AddDays(1); // 26. forrige måned

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
                        // Prøv WorkHoursInMonths først
                        (
                            from w in _db.WorkHoursInMonths
                            where w.EmployeeId == e.EmployeeId
                                  && w.PeriodStart == periodStart
                                  && w.PeriodEnd == periodEnd
                            select (decimal?)w.TotalHours
                        ).FirstOrDefault()
                        ??
                        // Ellers beregn ud fra ListOfShift + Substituteds
                        (
                            from s in _db.ListOfShift
                            join sub in _db.Substituteds
                                on s.SubstitutedId equals sub.SubstitutedId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where s.DateOfShift >= periodStart
                                  && s.DateOfShift <= periodEnd
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
                            join sub in _db.Substituteds
                                on s.SubstitutedId equals sub.SubstitutedId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where s.DateOfShift >= periodStart
                                  && s.DateOfShift <= periodEnd
                                  && s.SubstitutedId != null
                                  && sub != null
                                  && sub.EmployeeId == e.EmployeeId
                            select 1
                        ).Any()
                };

            return await query.ToListAsync(ct);
        }
    }
}
