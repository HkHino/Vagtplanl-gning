using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Services;

public class OutboxSyncWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OutboxSyncWorker> _logger;

    public OutboxSyncWorker(
        IServiceProvider services,
        ILogger<OutboxSyncWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox sync worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox sync cycle failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mongo = scope.ServiceProvider.GetRequiredService<MongoProjectionService>();

        var events = await db.OutboxEvents
            .Where(e => e.ProcessedUtc == null)
            .OrderBy(e => e.CreatedUtc)
            .Take(20)
            .ToListAsync(ct);

        foreach (var evt in events)
        {
            try
            {
                await HandleEventAsync(evt, db, mongo, ct);

                evt.ProcessedUtc = DateTime.UtcNow;
                evt.LastError = null;

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                evt.RetryCount++;
                evt.LastError = ex.Message;

                await db.SaveChangesAsync(ct);

                _logger.LogWarning(ex,
                    "Failed processing outbox event {EventId}", evt.Id);
            }
        }
    }

    private async Task HandleEventAsync(
        OutboxEvent evt,
        AppDbContext db,
        MongoProjectionService mongo,
        CancellationToken ct)
    {
        switch (evt.AggregateType)
        {
            case "Employee":
                await HandleEmployee(evt, db, mongo, ct);
                break;

            case "Bicycle":
                await HandleBicycle(evt, db, mongo, ct);
                break;

            case "Route":
                await HandleRoute(evt, db, mongo, ct);
                break;

            case "Shift":
                await HandleShift(evt, db, mongo, ct);
                break;

            case "WorkHours":
                await HandleWorkHours(evt, db, mongo, ct);
                break;

            case "Substituted":
                await HandleSubstituted(evt, db, mongo, ct);
                break;

            default:
                _logger.LogWarning(
                    "Unknown AggregateType '{AggregateType}' for OutboxEvent {Id}",
                    evt.AggregateType,
                    evt.Id
                );
                break;
        }
    }

    // ---------------- HANDLERS ----------------

    private async Task HandleEmployee(
        OutboxEvent evt,
        AppDbContext db,
        MongoProjectionService mongo,
        CancellationToken ct)
    {
        if (evt.EventType == "Deleted")
        {
            await mongo.DeleteEmployeeAsync(evt.AggregateId, ct);
            return;
        }

        var employee = await db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == evt.AggregateId, ct);

        if (employee != null)
        {
            await mongo.UpsertEmployeeAsync(employee, ct);
        }
    }

    private async Task HandleBicycle(
        OutboxEvent evt,
        AppDbContext db,
        MongoProjectionService mongo,
        CancellationToken ct)
    {
        if (evt.EventType == "Deleted")
        {
            await mongo.DeleteBicycleAsync(evt.AggregateId, ct);
            return;
        }

        var bicycle = await db.Bicycles
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BicycleId == evt.AggregateId, ct);

        if (bicycle != null)
        {
            await mongo.UpsertBicycleAsync(bicycle, ct);
        }
    }

    private async Task HandleRoute(
        OutboxEvent evt,
        AppDbContext db,
        MongoProjectionService mongo,
        CancellationToken ct)
    {
        if (evt.EventType == "Deleted")
        {
            await mongo.DeleteRouteAsync(evt.AggregateId, ct);
            return;
        }

        var route = await db.Routes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == evt.AggregateId, ct);

        if (route != null)
        {
            await mongo.UpsertRouteAsync(route, ct);
        }
    }

    private async Task HandleShift(
        OutboxEvent evt,
        AppDbContext db,
        MongoProjectionService mongo,
        CancellationToken ct)
    {
        if (evt.EventType == "Deleted")
        {
            await mongo.DeleteShiftAsync(evt.AggregateId, ct);
            return;
        }

        var shift = await db.ListOfShift
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ShiftId == evt.AggregateId, ct);

        if (shift != null)
        {
            await mongo.UpsertShiftAsync(shift, ct);
        }
    }

    private async Task HandleWorkHours(
        OutboxEvent evt,
        AppDbContext db,
        MongoProjectionService mongo,
        CancellationToken ct)
    {
        var workHours = await db.WorkHoursInMonths
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WorkHoursInMonthId == evt.AggregateId, ct);

        if (workHours != null)
        {
            await mongo.UpsertWorkHoursAsync(workHours, ct);
        }
    }

    private async Task HandleSubstituted(
        OutboxEvent evt,
        AppDbContext db,
        MongoProjectionService mongo,
        CancellationToken ct)
    {
        if (evt.EventType == "Deleted")
        {
            await mongo.DeleteSubstitutedAsync(evt.AggregateId, ct);
            return;
        }

        var substituted = await db.Substituteds
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SubstitutedId == evt.AggregateId, ct);

        if (substituted != null)
        {
            await mongo.UpsertSubstitutedAsync(substituted, ct);
        }
    }
}
