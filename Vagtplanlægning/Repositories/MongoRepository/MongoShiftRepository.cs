// fil: Repositories/MongoShiftRepository.cs
using MongoDB.Driver;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MongoShiftRepository : IShiftRepository
    {
        private readonly IMongoCollection<Shift> _shifts;
        private readonly IMongoCollection<Substituted> _substituteds;

        public MongoShiftRepository(IMongoDatabase database)
        {
            // Samme navne som tabellerne i MySQL
            _shifts = database.GetCollection<Shift>("ListOfShift");
            _substituteds = database.GetCollection<Substituted>("Substituteds");
        }

        public async Task<IEnumerable<Shift>> GetAllAsync(CancellationToken ct = default)
        {
            var cursor = await _shifts
                .FindAsync(Builders<Shift>.Filter.Empty, cancellationToken: ct);

            return await cursor.ToListAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var filter = Builders<Shift>.Filter.Eq(e => e.ShiftId, id);
            var result = await _shifts.DeleteOneAsync(filter, ct);
            return result.DeletedCount > 0;
        }

        public async Task<Shift?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var filter = Builders<Shift>.Filter.Eq(s => s.ShiftId, id);
            return await _shifts.Find(filter).FirstOrDefaultAsync(ct);
        }

        public async Task AddAsync(Shift shift, CancellationToken ct = default)
        {
            // Her antager vi, at ShiftId enten:
            // - sættes af app'en, eller
            // - genereres på anden vis (du kan tilføje logik her senere)
            await _shifts.InsertOneAsync(shift, cancellationToken: ct);
        }

        public async Task UpdateAsync(Shift shift, CancellationToken ct = default)
        {
            var filter = Builders<Shift>.Filter.Eq(s => s.ShiftId, shift.ShiftId);
            var result = await _shifts.ReplaceOneAsync(filter, shift, cancellationToken: ct);

            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException(
                    $"Shift with id {shift.ShiftId} not found in MongoDB.");
            }
        }

        public async Task MarkShiftSubstitutedAsync(
            int shiftId,
            bool hasSubstituted,
            CancellationToken ct = default)
        {
            // 1) Find shift
            var shift = await GetByIdAsync(shiftId, ct);
            if (shift == null)
            {
                throw new InvalidOperationException(
                    $"Shift with id {shiftId} not found in MongoDB.");
            }

            var subsId = shift.SubstitutedId;

            // 2) Opdater Substituteds
            var filter = Builders<Substituted>.Filter.Eq(s => s.SubstitutedId, subsId);
            var update = Builders<Substituted>.Update.Set(s => s.HasSubstituted, hasSubstituted);

            var result = await _substituteds.UpdateOneAsync(filter, update, cancellationToken: ct);

            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException(
                    $"Substituteds record with id {subsId} not found for shift {shiftId} in MongoDB.");
            }
        }
    }
}
