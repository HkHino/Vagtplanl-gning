namespace Vagtplanlægning.Repositories
{
    using Vagtplanlægning.Models;

    public interface IShiftRepository
    {
        Task<IEnumerable<Shift>> GetByDayAsync(DateTime day, CancellationToken ct = default);
        Task<Shift?> GetByIdAsync(int shiftId, CancellationToken ct = default);
        Task AddAsync(Shift shift, CancellationToken ct = default);
        Task UpdateAsync(Shift shift, CancellationToken ct = default);
    }
}
