using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models
{
    [BsonIgnoreExtraElements] // ignorér felter vi ikke har properties til
    public class Bicycle
    {
        // MongoDBs _id
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [NotMapped] // EF skal IKKE prøve at mappe denne til MySQL
        public string? MongoId { get; set; }

        // Dette felt hedder "Id" i Mongo, men "id" i MySQL-tabellen
        [BsonElement("Id")]
        public int BicycleId { get; set; }

        // Matcher "BicycleNumber" i Mongo og "bicycleNumber" i MySQL
        [BsonElement("BicycleNumber")]
        public int BicycleNumber { get; set; }

        [BsonElement("InOperate")]
        public bool InOperate { get; set; }

        // Navigation til shifts (bruges kun af EF – ignorér for Mongo)
        [NotMapped]
        public ICollection<Shift>? Shifts { get; set; }
    }
}
