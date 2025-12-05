using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;

namespace Vagtplanlægning.Services
{
    public class MongoMonthlyHoursReportService : IMonthlyHoursReportService
    {
        private readonly MongoDbContext _mongo;

        public MongoMonthlyHoursReportService(MongoDbContext mongo)
        {
            _mongo = mongo;
        }

        public async Task<List<MonthlyHoursRow>> GetMonthlyHoursAsync(
            int? employeeId,
            int year,
            int month,
            CancellationToken ct = default)
        {
            var periodEnd = new DateTime(year, month, 25);
            var periodStart = periodEnd.AddMonths(-1).AddDays(1);

            var whQuery = _mongo.WorkHoursInMonths
                .AsQueryable()
                .Where(w => w.PeriodStart == periodStart && w.PeriodEnd == periodEnd);

            if (employeeId.HasValue)
            {
                var id = employeeId.Value;
                whQuery = whQuery.Where(w => w.EmployeeId == id);
            }

            var whList = await whQuery.ToListAsync(ct);

            if (whList.Count == 0)
                return new List<MonthlyHoursRow>();

            var empIds = whList.Select(w => w.EmployeeId).Distinct().ToList();

            var empList = await _mongo.Employees
                .AsQueryable()
                .Where(e => empIds.Contains(e.EmployeeId))
                .ToListAsync(ct);

            var empDict = empList.ToDictionary(e => e.EmployeeId);

            return whList.Select(w =>
            {
                empDict.TryGetValue(w.EmployeeId, out var emp);

                return new MonthlyHoursRow
                {
                    EmployeeId = w.EmployeeId,
                    FirstName = emp?.FirstName ?? "",
                    LastName = emp?.LastName ?? "",
                    Year = year,
                    Month = month,
                    TotalMonthlyHours = w.TotalHours,
                    HasSubstituted = w.HasSubstituted
                };
            }).ToList();
        }
    }
}
