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

        public async Task MarkShiftSubstitutedAsync(int shiftId, bool hasSubstituted)
        {
            // 1) Find the shift
            var shift = await _db.ListOfShift
                .SingleOrDefaultAsync(s => s.ShiftId == shiftId);

            if (shift == null)
            {
                throw new KeyNotFoundException($"Shift with id {shiftId} not found.");
        }

            // If you ever use 0 as "no substitute", you can guard like this:
            if (shift.SubstitutedId == 0)
        {
                throw new InvalidOperationException(
                    $"Shift {shiftId} has substitutedId = 0, cannot update hasSubstituted.");
        }

            var subsId = shift.SubstitutedId;

            // 2) Find the Substituted row
            var subs = await _db.Substituteds
                .SingleOrDefaultAsync(s => s.SubstitutedId == subsId);

            if (subs == null)
        {
                throw new InvalidOperationException(
                    $"Substituteds record with id {subsId} not found for shift {shiftId}.");
        }

            // 3) Update flag
            subs.HasSubstituted = hasSubstituted;
            await _db.SaveChangesAsync();
        }

    }
}
