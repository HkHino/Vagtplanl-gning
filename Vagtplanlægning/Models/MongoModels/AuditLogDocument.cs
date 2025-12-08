using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models.MongoModels;

[BsonIgnoreExtraElements]
public class AuditLogDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
    
    [BsonElement("mysqlId")]
    public int MysqlId { get; set; }

    [BsonElement("tableName")]
    public string TableName { get; set; } = null!;

    // Typically the original table's primary key (string is correct)
    [BsonElement("recordId")]
    public string RecordId { get; set; } = null!;

    [BsonElement("action")]
    public string Action { get; set; } = null!; // "INSERT", "UPDATE", "DELETE"

    [BsonElement("changedAt")]
    public DateTime ChangedAt { get; set; }

    [BsonElement("changedBy")]
    [BsonIgnoreIfNull]
    public string? ChangedBy { get; set; }

    // Stored as raw JSON strings
    [BsonElement("oldData")]
    [BsonIgnoreIfNull]
    public string? OldData { get; set; }

    [BsonElement("newData")]
    [BsonIgnoreIfNull]
    public string? NewData { get; set; }
}