using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson.Serialization.Attributes;

namespace Vagtplanlægning.Models
{
    [Table("WorkHoursInMonths")]
    [BsonIgnoreExtraElements] // ignorer felter i Mongo som vi ikke har properties til
    [BsonNoId]                // sig eksplicit: denne klasse har ingen Id-property
    public class WorkHoursInMonth
    {
        [Column("workHoursInMonthId")]
        public int WorkHoursInMonthId { get; set; }

        [Column("employeeId")]
        public int EmployeeId { get; set; }

        [Column("payrollYear")]
        public int PayrollYear { get; set; }

        [Column("payrollMonth")]
        public int PayrollMonth { get; set; }

        [Column("periodStart")]
        public DateTime PeriodStart { get; set; }

        [Column("periodEnd")]
        public DateTime PeriodEnd { get; set; }

        [Column("totalHours")]
        public decimal TotalHours { get; set; }

        [Column("hasSubstituted")]
        public bool HasSubstituted { get; set; }
    }
}
