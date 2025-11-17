using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories;

public class MySqlShiftRepository : IShiftRepository
{
    private readonly AppDbContext _db;

    public MySqlShiftRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task MarkShiftSubstitutedAsync(int shiftId, bool hasSubstituted)
    {
        var shift = await _db.Shifts
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.ShiftId == shiftId);

        if (shift == null)
            throw new InvalidOperationException($"No shift found with id {shiftId}.");

        if (!shift.SubstitutedId.HasValue)
            throw new InvalidOperationException("This shift has no substitutedId set, cannot mark substitution.");

        var substitutedId = shift.SubstitutedId.Value;

        var substituted = await _db.Substituteds
            .SingleOrDefaultAsync(s => s.SubstitutedId == substitutedId);

        if (substituted == null)
            throw new InvalidOperationException($"No Substituted row found for substitutedId {substitutedId}.");

        substituted.HasSubstituted = hasSubstituted;

        await _db.SaveChangesAsync();
    }
}
