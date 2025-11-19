namespace Vagtplanlægning.Repositories
{
    using Vagtplanlægning.Models;

    public interface IShiftRepository
    {
        Task MarkShiftSubstitutedAsync(int shiftId, bool hasSubstituted);
    }
}
