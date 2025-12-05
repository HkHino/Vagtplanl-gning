using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models
{
    [Table("Route")]
    [BsonIgnoreExtraElements] // ignorer felter i Mongo, vi ikke har i klassen
    public class RouteEntity
    {
        // Mongo _id (ObjectId) – EF skal ignorere den
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [NotMapped]
        public string? MongoId { get; set; }

        // Fælles nøgle:
        //  - EF: kolonnen "id" i MySQL
        //  - Mongo: feltet "Id" 
        [Key]
        [Column("id")]
        [BsonElement("Id")]
        public int Id { get; set; }

        // RouteNumber:
        //  - EF: "routeNumber"
        //  - Mongo: "RouteNumber"
        [Column("routeNumber")]
        [BsonElement("RouteNumber")]
        public int RouteNumber { get; set; }

        // Relation til shifts (kun brugt af EF)
        public List<Shift> Shifts { get; set; } = new();
    }
}
