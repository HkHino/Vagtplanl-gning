using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MySqlShiftRepository : IShiftRepository
    {
        private readonly AppDbContext _db;

        public MySqlShiftRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Shift>> GetByDayAsync(DateTime day, CancellationToken ct = default)
        {
            // join to Days to filter by actual date
            var dayEntity = await _db.Days.FirstOrDefaultAsync(d => d.Day == day.Date, ct);
            if (dayEntity == null) return Enumerable.Empty<Shift>();

            return await _db.ListOfShift
                .Where(s => s.DayId == dayEntity.DayId)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Shift?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.ListOfShift.FirstOrDefaultAsync(s => s.ShiftId == id, ct);
        }

        public async Task AddAsync(Shift shift, CancellationToken ct = default)
        {
            _db.ListOfShift.Add(shift);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Shift shift, CancellationToken ct = default)
        {
            _db.ListOfShift.Update(shift);
            await _db.SaveChangesAsync(ct);
        }
    }
}
