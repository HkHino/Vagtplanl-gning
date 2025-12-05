using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;
using System.Threading;

namespace Vagtplanlægning.Repositories
{
    public class MySqlShiftRepository : IShiftRepository
    {
        private readonly AppDbContext _db;

        public MySqlShiftRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Shift?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.ListOfShift
                .SingleOrDefaultAsync(s => s.ShiftId == id, ct);
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

        public async Task MarkShiftSubstitutedAsync(int shiftId, bool hasSubstituted, CancellationToken ct = default)
        {
            // 1) Find the shift
            var shift = await _db.ListOfShift
                .SingleOrDefaultAsync(s => s.ShiftId == shiftId, ct);

            if (shift == null)
            {
                throw new InvalidOperationException(
                    $"Shift with id {shiftId} not found.");
            }

            // 2) Get the substitutedId from the shift
            var subsId = shift.SubstitutedId;

            // If there’s no substituted entry associated, we can either:
            // - silently do nothing, or
            // - throw. I’ll throw to surface data issues.
            var subs = await _db.Substituteds
                .SingleOrDefaultAsync(s => s.SubstitutedId == subsId, ct);

            if (subs == null)
            {
                throw new InvalidOperationException(
                    $"Substituteds record with id {subsId} not found for shift {shiftId}.");
            }

            // 3) Update flag
            subs.HasSubstituted = hasSubstituted;
            await _db.SaveChangesAsync(ct);
        }
    }
}
