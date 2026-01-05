namespace Vagtplanlægning.Models;

public class OutboxEvent
{
    public long Id { get; set; }

    public string AggregateType { get; set; } = null!;
    public int AggregateId { get; set; }

    public string EventType { get; set; } = null!;
    // Created | Updated | Deleted

    public string? PayloadJson { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime? ProcessedUtc { get; set; }

    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
