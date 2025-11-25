using System;
using System.Threading;
using System.Threading.Tasks;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Services
{
    public interface IShiftPlanService
    {
        // Genererer en fast 6-ugers plan ud fra startdato
        Task<ShiftPlan> Generate6WeekPlanAsync(
            DateTime startDate,
            CancellationToken ct = default);

        // Genererer en plan for en vilkårlig periode
        Task<ShiftPlan> GeneratePlanAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);
    }
}
