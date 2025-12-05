using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    /// <summary>
    /// Fallback-repo for shifts:
    /// - Prøver altid MySQL først
    /// - Hvis MySQL fejler (f.eks. DB nede), logger vi og bruger Mongo i stedet
    /// </summary>
    public class FallbackShiftRepository : IShiftRepository
    {
        private readonly MySqlShiftRepository _primary;
        private readonly MongoShiftRepository _fallback;
        private readonly ILogger<FallbackShiftRepository> _logger;

        public FallbackShiftRepository(
            MySqlShiftRepository primary,
            MongoShiftRepository fallback,
            ILogger<FallbackShiftRepository> logger)
        {
            _primary = primary;
            _fallback = fallback;
            _logger = logger;
        }

        // Helper til "T return" (GetByIdAsync)
        private async Task<T> ExecuteWithFallbackAsync<T>(
            Func<MySqlShiftRepository, Task<T>> primaryAction,
            Func<MongoShiftRepository, Task<T>> fallbackAction,
            CancellationToken ct = default)
        {
            try
            {
                return await primaryAction(_primary);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "MySQL unavailable - using MongoDB fallback for shifts (read).");
                return await fallbackAction(_fallback);
            }
        }

        // Helper til "void/Task" (Add/Update/MarkShiftSubstituted)
        private async Task ExecuteWithFallbackAsync(
            Func<MySqlShiftRepository, Task> primaryAction,
            Func<MongoShiftRepository, Task> fallbackAction,
            CancellationToken ct = default)
        {
            try
            {
                await primaryAction(_primary);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "MySQL unavailable - using MongoDB fallback for shifts (write).");
                await fallbackAction(_fallback);
            }
        }

        // ----------------------------------------------------
        // IShiftRepository-implementation
        // ----------------------------------------------------

        public Task<Shift?> GetByIdAsync(int id, CancellationToken ct = default)
            => ExecuteWithFallbackAsync(
                p => p.GetByIdAsync(id, ct),
                f => f.GetByIdAsync(id, ct),
                ct);

        public Task AddAsync(Shift shift, CancellationToken ct = default)
            => ExecuteWithFallbackAsync(
                p => p.AddAsync(shift, ct),
                f => f.AddAsync(shift, ct),
                ct);

        public Task UpdateAsync(Shift shift, CancellationToken ct = default)
            => ExecuteWithFallbackAsync(
                p => p.UpdateAsync(shift, ct),
                f => f.UpdateAsync(shift, ct),
                ct);

        public Task MarkShiftSubstitutedAsync(int shiftId, bool hasSubstituted, CancellationToken ct = default)
            => ExecuteWithFallbackAsync(
                p => p.MarkShiftSubstitutedAsync(shiftId, hasSubstituted, ct),
                f => f.MarkShiftSubstitutedAsync(shiftId, hasSubstituted, ct),
                ct);
    }
}
