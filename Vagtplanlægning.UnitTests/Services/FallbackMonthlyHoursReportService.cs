using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vagtplanlægning.DTOs;

namespace Vagtplanlægning.Services
{
    public class FallbackMonthlyHoursReportService : IMonthlyHoursReportService
    {
        private readonly IMonthlyHoursReportService _mysql;
        private readonly IMonthlyHoursReportService _mongo;
        private readonly ILogger<FallbackMonthlyHoursReportService> _logger;

        // TEST-venlig constructor (interfaces)
        public FallbackMonthlyHoursReportService(
            IMonthlyHoursReportService mysql,
            IMonthlyHoursReportService mongo,
            ILogger<FallbackMonthlyHoursReportService> logger)
        {
            _mysql = mysql ?? throw new ArgumentNullException(nameof(mysql));
            _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // DI-venlig convenience-constructor (konkrete typer)
        public FallbackMonthlyHoursReportService(
            MySqlMonthlyHoursReportService mysql,
            MongoMonthlyHoursReportService mongo,
            ILogger<FallbackMonthlyHoursReportService> logger)
            : this((IMonthlyHoursReportService)mysql,
                   (IMonthlyHoursReportService)mongo,
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
                // Prøv MySQL først
                return await _mysql.GetMonthlyHoursAsync(employeeId, year, month, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "MySQL unavailable - using MongoDB fallback for MonthlyHours (employee {EmployeeId}, {Year}-{Month})",
                    employeeId, year, month
                );

                // Fallback til Mongo
                return await _mongo.GetMonthlyHoursAsync(employeeId, year, month, ct);
            }
        }
    }
}
