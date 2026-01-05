using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Services;

public class OutboxWriter
{
    private readonly AppDbContext _db;

    public OutboxWriter(AppDbContext db)
    {
        _db = db;
    }

    public void AddEvent(
        string aggregateType,
        int aggregateId,
        string eventType,
        string? payloadJson = null)
    {
        var evt = new OutboxEvent
        {
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            EventType = eventType,
            PayloadJson = payloadJson,
            CreatedUtc = DateTime.UtcNow
        };

        _db.OutboxEvents.Add(evt);
    }
}
