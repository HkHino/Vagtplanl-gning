using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Services
{  
    /// <summary>
    /// Domæneservice for vagtplaner.
    /// Indeholder kun forretningslogik – ingen direkte DbContext.
    /// </summary>
    public class ShiftPlanService : IShiftPlanService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IRouteRepository _routeRepo;
        private readonly IShiftPlanRepository _planRepo;

        public ShiftPlanService(
            IEmployeeRepository employeeRepo,
            IRouteRepository routeRepo,
            IShiftPlanRepository planRepo)
        {
            _employeeRepo = employeeRepo;
            _routeRepo = routeRepo;
            _planRepo = planRepo;
        }

        public async Task<ShiftPlan> Generate6WeekPlanAsync(DateTime startDate, CancellationToken ct = default)
        {
            // 1) Hent data vi skal bruge
            var employees = (await _employeeRepo.GetAllAsync(ct)).ToList();
            var routesFromDb = (await _routeRepo.GetAllAsync(ct)).ToList();

            // 2) Ruter: sortér efter rutenummer. Fallback: 15 fiktive ruter hvis DB er tom.
            var routes = routesFromDb.Count > 0
                ? routesFromDb.OrderBy(r => r.RouteNumber).ToList()
                : Enumerable.Range(1, 15)
                    .Select(i => new RouteEntity
                    {
                        Id = 0,            // 0 = “kun i plan-snapshot”, ikke en rigtig DB-rute
                        RouteNumber = i
                    })
                    .ToList();

            // 3) 6 ugers periode (42 dage)
            var endDate = startDate.Date.AddDays(6 * 7);
            var allShifts = new List<Shift>();

            // Simpel logik: ruter 1-5 → level 1, 6-10 → level 2, resten → level 3
            var lvl1 = employees.Where(e => e.ExperienceLevel <= 1).ToList();
            var lvl2 = employees.Where(e => e.ExperienceLevel <= 2).ToList();
            var lvl3 = employees.Where(e => e.ExperienceLevel <= 3).ToList();

            int idx1 = 0, idx2 = 0, idx3 = 0;

            for (var day = startDate.Date; day < endDate; day = day.AddDays(1))
            {
                // Ingen weekend-vagter i auto-planen
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                foreach (var route in routes)
                {
                    var chosen = PickEmployeeForRoute(route.RouteNumber, lvl1, lvl2, lvl3, ref idx1, ref idx2, ref idx3);
                    if (chosen == null) continue;

                    var shift = new Shift
                    {
                        DateOfShift = day,
                        EmployeeId = chosen.EmployeeId,
                        BicycleId = 0,        // sættes manuelt senere i UI
                        SubstitutedId = 0,    // det samme
                        RouteId = route.Id,   // FK til Routes.id (eller 0 hvis fiktiv)
                        StartTime = null,
                        EndTime = null,
                        TotalHours = null
                    };

                    allShifts.Add(shift);
                }
            }

            var plan = new ShiftPlan
            {
                ShiftPlanId = Guid.NewGuid().ToString(),
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                Name = $"Vagtplan {startDate:dd-MM-yyyy} – {endDate:dd-MM-yyyy}",
                Shifts = allShifts
            };

            // Gem selve snapshot’et (ShiftPlanRow med JSON) i DB
            await _planRepo.AddAsync(plan, ct);

            return plan;
        }

        /// <summary>
        /// Meget simpel “routing”-logik på erfaring:
        ///  - ruter 1-5: level 1-medarbejdere
        ///  - ruter 6-10: level 2
        ///  - resten: level 3
        /// </summary>
        private Employee? PickEmployeeForRoute(
            int routeNumber,
            List<Employee> lvl1,
            List<Employee> lvl2,
            List<Employee> lvl3,
            ref int idx1,
            ref int idx2,
            ref int idx3)
        {
            if (routeNumber <= 5 && lvl1.Count > 0)
            {
                var e = lvl1[idx1 % lvl1.Count];
                idx1++;
                return e;
            }

            if (routeNumber <= 10 && lvl2.Count > 0)
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
        public async Task<ShiftPlan> GeneratePlanAsync(
    DateTime startDate,
    DateTime endDate,
    CancellationToken ct = default)
        {
            var start = startDate.Date;
            var end = endDate.Date;

            if (end <= start)
            {
                throw new ArgumentException("endDate must be after startDate", nameof(endDate));
            }

            // 1) Hent data vi skal bruge
            var employees = (await _employeeRepo.GetAllAsync(ct)).ToList();
            var routesFromDb = (await _routeRepo.GetAllAsync(ct)).ToList();

            // 2) Ruter: sortér efter rutenummer. Fallback: 15 fiktive ruter hvis DB er tom.
            var routes = routesFromDb.Count > 0
                ? routesFromDb.OrderBy(r => r.RouteNumber).ToList()
                : Enumerable.Range(1, 15)
                    .Select(i => new RouteEntity
                    {
                        Id = 0,            // 0 = “kun i plan-snapshot”, ikke en rigtig DB-rute
                        RouteNumber = i
                    })
                    .ToList();

            // 3) Periode: fra start til end (ikke inkl. end)
            var allShifts = new List<Shift>();

            // Simpel logik: ruter 1-5 → level 1, 6-10 → level 2, resten → level 3
            var lvl1 = employees.Where(e => e.ExperienceLevel <= 1).ToList();
            var lvl2 = employees.Where(e => e.ExperienceLevel <= 2).ToList();
            var lvl3 = employees.Where(e => e.ExperienceLevel <= 3).ToList();

            int idx1 = 0, idx2 = 0, idx3 = 0;

            for (var day = start; day < end; day = day.AddDays(1))
            {
                // Ingen weekend-vagter i auto-planen
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                foreach (var route in routes)
                {
                    var chosen = PickEmployeeForRoute(
                        route.RouteNumber,
                        lvl1, lvl2, lvl3,
                        ref idx1, ref idx2, ref idx3);

                    if (chosen == null) continue;

                    var shift = new Shift
                    {
                        DateOfShift = day,
                        EmployeeId = chosen.EmployeeId,
                        BicycleId = 0,        // sættes manuelt senere i UI
                        SubstitutedId = 0,    // det samme
                        RouteId = route.Id,   // FK til Routes.id (eller 0 hvis fiktiv)
                        StartTime = null,
                        EndTime = null,
                        TotalHours = null
                    };

                    allShifts.Add(shift);
                }
            }

            var plan = new ShiftPlan
            {
                ShiftPlanId = Guid.NewGuid().ToString(),
                StartDate = start,
                EndDate = end,
                Name = $"Vagtplan {start:dd-MM-yyyy} – {end:dd-MM-yyyy}",
                Shifts = allShifts
            };

            // Gem selve snapshot’et (ShiftPlanRow med JSON) i DB
            await _planRepo.AddAsync(plan, ct);

            return plan;
        }

    }

}
