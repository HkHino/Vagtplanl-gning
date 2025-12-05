using Microsoft.Extensions.Logging;
using Vagtplanlægning.DTOs;

namespace Vagtplanlægning.Services
{
    public class FallbackMonthlyHoursReportService : IMonthlyHoursReportService
    {
        private readonly MySqlMonthlyHoursReportService _primary;
        private readonly MongoMonthlyHoursReportService _fallback;
        private readonly ILogger<FallbackMonthlyHoursReportService> _logger;

        public FallbackMonthlyHoursReportService(
            MySqlMonthlyHoursReportService primary,
            MongoMonthlyHoursReportService fallback,
            ILogger<FallbackMonthlyHoursReportService> logger)
        {
            _primary = primary;
            _fallback = fallback;
            _logger = logger;
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
                _logger.LogWarning(ex,
                    "MySQL unavailable - using MongoDB fallback for MonthlyHours.");
                return await _fallback.GetMonthlyHoursAsync(employeeId, year, month, ct);
            }
        }
    }
}
