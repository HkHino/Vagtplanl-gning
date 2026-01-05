using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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

        public async Task<IEnumerable<Shift>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.ListOfShift
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Shift?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.ListOfShift
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.ShiftId == id, ct);
        }

        public async Task AddAsync(Shift shift, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            _db.ListOfShift.Add(shift);
            await _db.SaveChangesAsync(ct);

            _db.OutboxEvents.Add(new OutboxEvent
            {
                AggregateType = "Shift",
                AggregateId = shift.ShiftId,
                EventType = "Created",
                CreatedUtc = DateTime.UtcNow,
                PayloadJson = null
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        public async Task UpdateAsync(Shift shift, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            _db.ListOfShift.Update(shift);
            await _db.SaveChangesAsync(ct);

            _db.OutboxEvents.Add(new OutboxEvent
            {
                AggregateType = "Shift",
                AggregateId = shift.ShiftId,
                EventType = "Updated",
                CreatedUtc = DateTime.UtcNow,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    shift.ShiftId
                })
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            var existing = await _db.ListOfShift
                .FirstOrDefaultAsync(e => e.ShiftId == id, ct);

            if (existing == null)
                return false;

            _db.ListOfShift.Remove(existing);
            await _db.SaveChangesAsync(ct);

            _db.OutboxEvents.Add(new OutboxEvent
            {
                AggregateType = "Shift",
                AggregateId = id,
                EventType = "Deleted",
                CreatedUtc = DateTime.UtcNow,
                PayloadJson = null
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return true;
        }

        public async Task MarkShiftSubstitutedAsync(
            int shiftId,
            bool hasSubstituted,
            CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            var shift = await _db.ListOfShift
                .SingleOrDefaultAsync(s => s.ShiftId == shiftId, ct);

            if (shift == null)
                throw new InvalidOperationException(
                    $"Shift with id {shiftId} not found.");

            var subs = await _db.Substituteds
                .SingleOrDefaultAsync(
                    s => s.SubstitutedId == shift.SubstitutedId,
                    ct);

            if (subs == null)
                throw new InvalidOperationException(
                    $"Substituteds record with id {shift.SubstitutedId} not found.");

            subs.HasSubstituted = hasSubstituted;
            await _db.SaveChangesAsync(ct);

            _db.OutboxEvents.Add(new OutboxEvent
            {
                AggregateType = "Substituted",
                AggregateId = subs.SubstitutedId,
                EventType = "Updated",
                CreatedUtc = DateTime.UtcNow,
                PayloadJson = null
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
    }
}
