using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Services
{
    public interface IShiftPlanService
    {
        Task<ShiftPlan> Generate6WeekPlanAsync(DateTime startDate, CancellationToken ct = default);
    }

    public class ShiftPlanService : IShiftPlanService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IRouteRepository _routeRepo;
        private readonly IShiftPlanRepository _planRepo;

        public ShiftPlanService(IEmployeeRepository employeeRepo, IRouteRepository routeRepo, IShiftPlanRepository planRepo)
        {
            _employeeRepo = employeeRepo;
            _routeRepo = routeRepo;
            _planRepo = planRepo;
        }

        public async Task<ShiftPlan> Generate6WeekPlanAsync(DateTime startDate, CancellationToken ct = default)
        {
            var employees = (await _employeeRepo.GetAllAsync(ct)).ToList();
            var routesFromDb = (await _routeRepo.GetAllAsync(ct)).ToList();

            var routes = routesFromDb.Count > 0
                ? routesFromDb.OrderBy(r => r.RouteNumberId).ToList()
                : Enumerable.Range(1, 15).Select(i => new RouteEntity { RouteNumberId = i }).ToList();

            var endDate = startDate.Date.AddDays(6 * 7);
            var allShifts = new List<Shift>();

            var lvl1 = employees.Where(e => e.ExperienceLevel <= 1).ToList();
            var lvl2 = employees.Where(e => e.ExperienceLevel <= 2).ToList();
            var lvl3 = employees.Where(e => e.ExperienceLevel <= 3).ToList();
            int idx1 = 0, idx2 = 0, idx3 = 0;

            for (var day = startDate.Date; day < endDate; day = day.AddDays(1))
            {
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                foreach (var route in routes)
                {
                    var chosen = PickEmployeeForRoute(route.RouteNumberId, lvl1, lvl2, lvl3, ref idx1, ref idx2, ref idx3);
                    if (chosen == null) continue;

                    var shift = new Shift
                    {
                        DayId = 0,
                        EmployeeId = chosen.EmployeeId,
                        BicycleId = 0,
                        SubstitutedId = 0,
                        RouteNumberId = route.RouteNumberId,
                        MeetInTime = new TimeSpan(8, 0, 0),
                        StartTime = null,
                        EndTime = null,
                        TotalHours = null
                    };
                    allShifts.Add(shift);
                }
            }

            var plan = new ShiftPlan
            {
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                Name = $"Plan {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                Shifts = allShifts
            };

            await _planRepo.AddAsync(plan, ct);
            return plan;
        }

        private Employee? PickEmployeeForRoute(
            int routeNumberId,
            List<Employee> lvl1,
            List<Employee> lvl2,
            List<Employee> lvl3,
            ref int idx1,
            ref int idx2,
            ref int idx3)
        {
            if (routeNumberId <= 5 && lvl1.Count > 0)
            {
                var e = lvl1[idx1 % lvl1.Count];
                idx1++;
                return e;
            }

            if (routeNumberId <= 10 && lvl2.Count > 0)
            {
                var e = lvl2[idx2 % lvl2.Count];
                idx2++;
                return e;
            }

            if (lvl3.Count > 0)
            {
                var e = lvl3[idx3 % lvl3.Count];
                idx3++;
                return e;
            }

            return null;
        }
    }
}
