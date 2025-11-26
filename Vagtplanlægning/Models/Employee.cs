using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace Vagtplanlægning.Models
{
    // Mongo må gerne ignorere felter vi ikke har properties til
    [BsonIgnoreExtraElements]
    public class Employee
    {
        // MongoDBs interne _id
        [NotMapped]             //<- vigtigt for EF skal ignorere den
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId? MongoId { get; set; }

        // Link til det oprindelige SQL-id i dine Mongo-dokumenter (sqlEmployeeId)
        [BsonElement("sqlEmployeeId")]
        public int EmployeeId { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; } = "";

        [BsonElement("lastName")]
        public string LastName { get; set; } = "";

        [BsonElement("address")]
        public string Address { get; set; } = "";

        [BsonElement("phone")]
        public string Phone { get; set; } = "";

        [BsonElement("email")]
        public string Email { get; set; } = "";

        [BsonElement("experienceLevel")]
        public int ExperienceLevel { get; set; } = 1;

        // ---------- EF navigation properties (bruges IKKE af Mongo) ----------

        [BsonIgnore]
        public ICollection<Shift> Shifts { get; set; } = new List<Shift>();

        [BsonIgnore]
        public ICollection<Substituted> SubstitutionRecords { get; set; } = new List<Substituted>();
    }
}