using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace Vagtplanlægning.Models
{
    /// <summary>
    /// Domain model for an employee.
    ///
    /// This class is used by:
    /// - EF Core / MySQL (<see cref="AppDbContext"/>).
    /// - MongoDB (for hybrid / fallback setups).
    ///
    /// Mongo-specific fields are annotated with <see cref="Bson*"/> attributes
    /// and EF-specific navigation properties are marked with <see cref="BsonIgnoreAttribute"/>
    /// so MongoDB ignores them.
    /// </summary>
    // Mongo is allowed to ignore fields that we do not have properties for
    [BsonIgnoreExtraElements]
    public class Employee
    {
        /// <summary>
        /// Internal MongoDB <c>_id</c>.
        ///
        /// Marked as <see cref="NotMapped"/> so EF Core ignores it, and used only when
        /// documents are stored in MongoDB.
        /// </summary>
        [NotMapped]             //<- important EF will ignore this
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId? MongoId { get; set; }

         /// <summary>
        /// Link back to the original SQL-based id, shared between MySQL and MongoDB.
        /// </summary>
        [BsonElement("employeeId")]
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

        // ---------- EF navigation properties (not used by Mongo) ----------

        /// <summary>
        /// Shifts assigned to this employee (EF Core navigation property).
        /// Ignored by MongoDB.
        /// </summary>
        [BsonIgnore]
        public ICollection<Shift> Shifts { get; set; } = new List<Shift>();

        /// <summary>
        /// Substitution records where this employee has been a substitute.
        /// Ignored by MongoDB.
        /// </summary>
        [BsonIgnore]
        public ICollection<Substituted> SubstitutionRecords { get; set; } = new List<Substituted>();
    }
}