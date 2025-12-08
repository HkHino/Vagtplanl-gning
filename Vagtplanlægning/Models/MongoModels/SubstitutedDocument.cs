using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models.MongoModels;

[BsonIgnoreExtraElements]
public class SubstitutedDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]        // <- important
    public string? Id { get; set; }

    // Keep business key from MySQL if you want it
    [BsonElement("substitutedId")]
    public int SubstitutedId { get; set; }

    // Reference to employee document
    [BsonElement("employeeRefId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string EmployeeRefId { get; set; } = null!;

    [BsonElement("hasSubstituted")]
    public bool HasSubstituted { get; set; } = false;
}