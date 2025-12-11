using Vagtplanlægning.DTOs;

namespace Vagtplanlægning.Services
{
    public interface IMonthlyHoursReportService
    {
        Task<List<MonthlyHoursRow>> GetMonthlyHoursAsync(
            int? employeeId,
            int year,
            int month,
            CancellationToken ct = default);
    }
}
