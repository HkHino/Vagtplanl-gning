using Vagtplanlægning.DTOs;

namespace Vagtplanlægning.Services
{
    public class FallbackMonthlyHoursReportService : IMonthlyHoursReportService
    {
        private readonly IMonthlyHoursReportService _primary;
        private readonly IMonthlyHoursReportService _fallback;
        private readonly ILogger<FallbackMonthlyHoursReportService> _logger;

        // ✅ Ny, generel + test-venlig constructor
        public FallbackMonthlyHoursReportService(
            IMonthlyHoursReportService primary,
            IMonthlyHoursReportService fallback,
            ILogger<FallbackMonthlyHoursReportService> logger)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ✅ Din gamle DI-constructor – stadig brugt af Program.cs
        //    Den kalder bare den nye constructor
        public FallbackMonthlyHoursReportService(
            MySqlMonthlyHoursReportService primary,
            MongoMonthlyHoursReportService fallback,
            ILogger<FallbackMonthlyHoursReportService> logger)
            : this((IMonthlyHoursReportService)primary,
                   (IMonthlyHoursReportService)fallback,
                   logger)
        {
        }

        public async Task<List<MonthlyHoursRow>> GetMonthlyHoursAsync(
            int? employeeId,
            int year,
            int month,
            CancellationToken ct = default)
        {
            try
            {
                return await _primary.GetMonthlyHoursAsync(employeeId, year, month, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "MySQL unavailable - using MongoDB fallback for MonthlyHours (employee {EmployeeId}, {Year}-{Month})",
                    employeeId, year, month
                );

                return await _fallback.GetMonthlyHoursAsync(employeeId, year, month, ct);
            }
        }
    }
}
