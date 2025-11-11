using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MySqlShiftPlanRepository : IShiftPlanRepository
    {
        private readonly AppDbContext _db;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public MySqlShiftPlanRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ShiftPlan>> GetAllAsync(CancellationToken ct = default)
        {
            var rows = await _db.ShiftPlans.AsNoTracking().ToListAsync(ct);

            // map each row to domain
            return rows.Select(MapToDomain).ToList();
        }

        public async Task<ShiftPlan?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var row = await _db.ShiftPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ShiftPlanId == id, ct);

            return row == null ? null : MapToDomain(row);
        }

        public async Task AddAsync(ShiftPlan plan, CancellationToken ct = default)
        {
            var row = MapToRow(plan);
            _db.ShiftPlans.Add(row);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        {
            var row = await _db.ShiftPlans.FindAsync(new object?[] { id }, ct);
            if (row == null) return false;
            _db.ShiftPlans.Remove(row);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        // helper: domain -> row
        private ShiftPlanRow MapToRow(ShiftPlan plan)
        {
            return new ShiftPlanRow
            {
                ShiftPlanId = plan.ShiftPlanId,
                Name = plan.Name,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                ShiftsJson = JsonSerializer.Serialize(plan.Shifts, _jsonOptions)
            };
        }

        // helper: row -> domain
        private ShiftPlan MapToDomain(ShiftPlanRow row)
        {
            List<Shift>? shifts = null;
            if (!string.IsNullOrWhiteSpace(row.ShiftsJson))
            {
                try
                {
                    shifts = JsonSerializer.Deserialize<List<Shift>>(row.ShiftsJson, _jsonOptions);
                }
                catch
                {
                    shifts = new List<Shift>();
                }
            }

            return new ShiftPlan
            {
                ShiftPlanId = row.ShiftPlanId,
                Name = row.Name,
                StartDate = row.StartDate,
                EndDate = row.EndDate,
                Shifts = shifts ?? new List<Shift>()
            };
        }
    }
}
