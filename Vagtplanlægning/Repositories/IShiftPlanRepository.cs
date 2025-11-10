namespace Vagtplanlægning.Repositories
{
    using Vagtplanlægning.Models;

    public interface IShiftPlanRepository
    {
        Task<IEnumerable<ShiftPlan>> GetAllAsync(CancellationToken ct = default);
        Task<ShiftPlan?> GetByIdAsync(string id, CancellationToken ct = default);
        Task AddAsync(ShiftPlan plan, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    }
}
