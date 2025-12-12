using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public interface IShiftRepository
    {
        // Get all shifts 
        Task<IEnumerable<Shift>> GetAllAsync(CancellationToken ct = default);
        
        // Delete a shift
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        
        // Simple get by id
        Task<Shift?> GetByIdAsync(int id, CancellationToken ct = default);

        // Insert a new shift
        Task AddAsync(Shift shift, CancellationToken ct = default);

        // Update an existing shift
        Task UpdateAsync(Shift shift, CancellationToken ct = default);

        // Update substituted flag for a shift
        Task MarkShiftSubstitutedAsync(int shiftId, bool hasSubstituted, CancellationToken ct = default);
    }
}
