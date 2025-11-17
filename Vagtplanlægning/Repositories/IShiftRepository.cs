namespace Vagtplanlægning.Repositories;

public interface IShiftRepository
{
    Task MarkShiftSubstitutedAsync(int shiftId, bool hasSubstituted);
}
