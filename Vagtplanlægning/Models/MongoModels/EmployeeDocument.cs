using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models.MongoModels;

[BsonIgnoreExtraElements]
public class EmployeeDocument
{
    // Mongo primary key
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
    
    [BsonElement("mysqlId")]
    public int MysqlId { get; set; }

    [BsonElement("firstName")]
    public string FirstName { get; set; } = null!;

    [BsonElement("lastName")]
    public string LastName { get; set; } = null!;

    [BsonElement("address")]
    public string Address { get; set; } = null!;

    [BsonElement("phone")]
    public string Phone { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("experienceLevel")]
    public int ExperienceLevel { get; set; } = 1;
}