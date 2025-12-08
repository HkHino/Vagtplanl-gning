using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models.MongoModels;

[BsonIgnoreExtraElements]
public class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull] 
    public string? Id { get; set; }

    [BsonElement("userId")]
    public int UserId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = null!;

    [BsonElement("role")]
    public string Role { get; set; } = null!;

    [BsonElement("hash")]
    public string Hash { get; set; } = null!;
    
    [BsonElement("employeeRefId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? EmployeeRefId { get; set; }
}