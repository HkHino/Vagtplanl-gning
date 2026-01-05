using MongoDB.Driver;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Services;

public class MongoProjectionService
{
    private readonly IMongoDatabase _mongo;
    private readonly IMongoCollection<Bicycle> _bicycles;

    public MongoProjectionService(IMongoDatabase mongo)
    {
        _mongo = mongo;
        _bicycles = _mongo.GetCollection<Bicycle>("Bicycles");
    }

    // EMPLOYEES -------------------------------------------------------------

    public async Task UpsertEmployeeAsync(Employee e, CancellationToken ct)
    {
        var collection = _mongo.GetCollection<Employee>("Employees");

        var filter = Builders<Employee>.Filter
            .Eq(x => x.EmployeeId, e.EmployeeId);

        await collection.ReplaceOneAsync(
            filter,
            e,
            new ReplaceOptions { IsUpsert = true },
            ct
        );
    }

    public async Task DeleteEmployeeAsync(int employeeId, CancellationToken ct)
    {
        var collection = _mongo.GetCollection<Employee>("Employees");

        await collection.DeleteOneAsync(
            Builders<Employee>.Filter.Eq(x => x.EmployeeId, employeeId),
            ct
        );
    }

    // BICYCLES --------------------------------------------------------------

    public async Task UpsertBicycleAsync(Bicycle b, CancellationToken ct)
    {
        var filter = Builders<Bicycle>.Filter
            .Eq(x => x.BicycleId, b.BicycleId);

        var update = Builders<Bicycle>.Update
            .Set(x => x.BicycleId, b.BicycleId)
            .Set(x => x.BicycleNumber, b.BicycleNumber)
            .Set(x => x.InOperate, b.InOperate);

        await _bicycles.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true },
            ct
        );
    }

    public async Task DeleteBicycleAsync(int bicycleId, CancellationToken ct)
    {
        await _bicycles.DeleteOneAsync(
            x => x.BicycleId == bicycleId,
            ct
        );
    }

    // ROUTES ----------------------------------------------------------------

    public async Task UpsertRouteAsync(RouteEntity r, CancellationToken ct)
    {
        var col = _mongo.GetCollection<RouteEntity>("Routes");

        await col.ReplaceOneAsync(
            x => x.Id == r.Id,
            r,
            new ReplaceOptions { IsUpsert = true },
            ct
        );
    }

    public async Task DeleteRouteAsync(int routeId, CancellationToken ct)
    {
        var col = _mongo.GetCollection<RouteEntity>("Routes");

        await col.DeleteOneAsync(x => x.Id == routeId, ct);
    }

    // SHIFTS ----------------------------------------------------------------

    public async Task UpsertShiftAsync(Shift s, CancellationToken ct)
    {
        var col = _mongo.GetCollection<Shift>("Shifts");

        await col.ReplaceOneAsync(
            x => x.ShiftId == s.ShiftId,
            s,
            new ReplaceOptions { IsUpsert = true },
            ct
        );
    }

    public async Task DeleteShiftAsync(int shiftId, CancellationToken ct)
    {
        var col = _mongo.GetCollection<Shift>("Shifts");

        await col.DeleteOneAsync(x => x.ShiftId == shiftId, ct);
    }

    // WORK HOURS ------------------------------------------------------------

    public async Task UpsertWorkHoursAsync(WorkHoursInMonth w, CancellationToken ct)
    {
        var col = _mongo.GetCollection<WorkHoursInMonth>("WorkHoursInMonth");

        await col.ReplaceOneAsync(
            x => x.WorkHoursInMonthId == w.WorkHoursInMonthId,
            w,
            new ReplaceOptions { IsUpsert = true },
            ct
        );
    }

    // SUBSTITUTEDS ----------------------------------------------------------

    public async Task UpsertSubstitutedAsync(Substituted s, CancellationToken ct)
    {
        var col = _mongo.GetCollection<Substituted>("Substituteds");

        await col.ReplaceOneAsync(
            x => x.SubstitutedId == s.SubstitutedId,
            s,
            new ReplaceOptions { IsUpsert = true },
            ct
        );
    }

    public async Task DeleteSubstitutedAsync(int substitutedId, CancellationToken ct)
    {
        var col = _mongo.GetCollection<Substituted>("Substituteds");

        await col.DeleteOneAsync(x => x.SubstitutedId == substitutedId, ct);
    }
}
