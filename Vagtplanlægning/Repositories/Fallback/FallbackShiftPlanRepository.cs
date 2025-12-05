using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    /// <summary>
    /// Fallback for ShiftPlan:
    /// - Prøver MySQL først
    /// - Hvis MySQL fejler (DB nede), logger vi og bruger Mongo i stedet
    /// </summary>
    public class FallbackShiftPlanRepository : IShiftPlanRepository
    {
        private readonly MySqlShiftPlanRepository _primary;
        private readonly MongoShiftPlanRepository _fallback;
        private readonly ILogger<FallbackShiftPlanRepository> _logger;

        public FallbackShiftPlanRepository(
            MySqlShiftPlanRepository primary,
            MongoShiftPlanRepository fallback,
            ILogger<FallbackShiftPlanRepository> logger)
        {
            _primary = primary;
            _fallback = fallback;
            _logger = logger;
        }

        // Hjælper til metoder der returnerer T
        private async Task<T> ExecuteWithFallbackAsync<T>(
            Func<MySqlShiftPlanRepository, Task<T>> primaryAction,
            Func<MongoShiftPlanRepository, Task<T>> fallbackAction,
            CancellationToken ct)
        {
            try
            {
                return await primaryAction(_primary);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "MySQL unavailable - using MongoDB fallback for ShiftPlans (read).");
                return await fallbackAction(_fallback);
            }
        }

        // Hjælper til metoder der returnerer Task/void
        private async Task ExecuteWithFallbackAsync(
            Func<MySqlShiftPlanRepository, Task> primaryAction,
            Func<MongoShiftPlanRepository, Task> fallbackAction,
            CancellationToken ct)
        {
            try
            {
                await primaryAction(_primary);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "MySQL unavailable - using MongoDB fallback for ShiftPlans (write).");
                await fallbackAction(_fallback);
            }
        }

        public Task<IEnumerable<ShiftPlan>> GetAllAsync(CancellationToken ct = default) =>
            ExecuteWithFallbackAsync(
                p => p.GetAllAsync(ct),
                f => f.GetAllAsync(ct),
                ct);

        public Task<ShiftPlan?> GetByIdAsync(string id, CancellationToken ct = default) =>
            ExecuteWithFallbackAsync(
                p => p.GetByIdAsync(id, ct),
                f => f.GetByIdAsync(id, ct),
                ct);

        public Task AddAsync(ShiftPlan plan, CancellationToken ct = default) =>
            ExecuteWithFallbackAsync(
                p => p.AddAsync(plan, ct),
                f => f.AddAsync(plan, ct),
                ct);

        public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        {
            try
            {
                return await _primary.DeleteAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "MySQL unavailable - using MongoDB fallback for ShiftPlans (delete).");
                return await _fallback.DeleteAsync(id, ct);
            }
        }

        public Task UpdateAsync(ShiftPlan plan, CancellationToken ct = default) =>
            ExecuteWithFallbackAsync(
                p => p.UpdateAsync(plan, ct),
                f => f.UpdateAsync(plan, ct),
                ct);
    }
}
