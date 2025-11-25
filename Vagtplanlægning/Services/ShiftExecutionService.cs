using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Services
{
    public interface IShiftExecutionService
    {
        Task AddShiftAsync(DateTime day, int employeeId, int bicycleId, int routeNumberId, TimeSpan meetInTime, int substitutedId, CancellationToken ct = default);
        Task SetStartTimeAsync(int shiftId, TimeSpan start, CancellationToken ct = default);
        Task SetEndTimeAsync(int shiftId, TimeSpan end, CancellationToken ct = default);
    }

    public class ShiftExecutionService : IShiftExecutionService
    {
        private readonly string _provider;
        private readonly AppDbContext? _db;
        private readonly IShiftRepository _shiftRepo;

        public ShiftExecutionService(IConfiguration config, IShiftRepository shiftRepo, AppDbContext? db = null)
        {
            _provider = config["DatabaseProvider"] ?? "MySql";
            _shiftRepo = shiftRepo;
            _db = db;
        }

        public async Task AddShiftAsync(DateTime day, int employeeId, int bicycleId, int routeNumberId, TimeSpan meetInTime, int substitutedId, CancellationToken ct = default)
        {
            if (_provider == "MySql" && _db != null)
            {
                await _db.Database.ExecuteSqlRawAsync(
                    "CALL AddShift({0},{1},{2},{3},{4},{5})",
                    day.Date, employeeId, bicycleId, routeNumberId, meetInTime, substitutedId);
            }
            else
            {
                var shift = new Shift
                {
                    DateOfShift = day,
                    EmployeeId = employeeId,
                    BicycleId = bicycleId,
                    RouteId = routeNumberId,                    
                    StartTime = null,
                    EndTime = null,
                    SubstitutedId = substitutedId,
                    TotalHours = null
                };
                await _shiftRepo.AddAsync(shift, ct);
            }
        }

        public async Task SetStartTimeAsync(int shiftId, TimeSpan start, CancellationToken ct = default)
        {
            if (_provider == "MySql" && _db != null)
            {
                await _db.Database.ExecuteSqlRawAsync("CALL SetStartTime({0},{1})", shiftId, start);
            }
            else
            {
                var shift = await _shiftRepo.GetByIdAsync(shiftId, ct);
                if (shift == null) return;
                shift.StartTime = start;
                await _shiftRepo.UpdateAsync(shift, ct);
            }
        }

        public async Task SetEndTimeAsync(int shiftId, TimeSpan end, CancellationToken ct = default)
        {
            if (_provider == "MySql" && _db != null)
            {
                await _db.Database.ExecuteSqlRawAsync("CALL SetEndTime({0},{1})", shiftId, end);
            }
            else
            {
                var shift = await _shiftRepo.GetByIdAsync(shiftId, ct);
                if (shift == null) return;
                shift.EndTime = end;

                if (shift.StartTime.HasValue)
                {
                    var total = end - shift.StartTime.Value;
                    shift.TotalHours = (decimal)total.TotalHours;
                }

                await _shiftRepo.UpdateAsync(shift, ct);
            }
        }
    }
}
