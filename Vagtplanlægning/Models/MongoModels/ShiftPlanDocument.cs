using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models.MongoModels;

[BsonIgnoreExtraElements]
public class ShiftPlanDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]        // <- important
    public string? Id { get; set; }

    [BsonElement("shiftPlanId")]
    public string ShiftPlanId { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    [BsonElement("endDate")]
    public DateTime EndDate { get; set; }

    // Stored exactly as a JSON string
    [BsonElement("shifts")]
    public string Shifts { get; set; } = null!;
}