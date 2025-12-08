using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models.MongoModels;

[BsonIgnoreExtraElements]
public class RouteDocument
{
    
    // Mongo primary key
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
    
    [BsonElement("mysqlId")]
    public int MysqlId { get; set; }

    [BsonElement("routeNumber")]
    public int RouteNumber { get; set; }
}