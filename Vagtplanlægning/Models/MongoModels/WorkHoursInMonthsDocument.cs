using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models.MongoModels;

[BsonIgnoreExtraElements]
public class WorkHoursInMonthsDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]        // <- important
    public string? Id { get; set; }
        
    [BsonElement("workHoursInMonthId")]
    public int WorkHoursInMonthId { get; set; }

    // Reference to employee (ObjectId)
    [BsonElement("employeeRefId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string EmployeeRefId { get; set; } = null!;

    [BsonElement("payrollYear")]
    public int PayrollYear { get; set; }

    [BsonElement("payrollMonth")]
    public int PayrollMonth { get; set; }

    [BsonElement("periodStart")]
    public DateTime PeriodStart { get; set; }

    [BsonElement("periodEnd")]
    public DateTime PeriodEnd { get; set; }

    [BsonElement("totalHours")]
    public decimal TotalHours { get; set; } = 0;

    [BsonElement("hasSubstituted")]
    public bool HasSubstituted { get; set; } = false;
}