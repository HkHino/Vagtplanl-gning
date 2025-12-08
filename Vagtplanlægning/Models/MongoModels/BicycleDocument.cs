using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models.MongoModels;

public class BicycleDocument
{
    // Mongo primary key
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
    
    [BsonElement("mysqlId")]
    public int MysqlId { get; set; }

    [BsonElement("bicycleNumber")]
    public int BicycleNumber { get; set; }

    [BsonElement("inOperate")]
    public bool InOperate { get; set; } = false;
}