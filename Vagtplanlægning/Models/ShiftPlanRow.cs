using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    /// <summary>
    /// Rå database-entity for ShiftPlans-tabellen.
    /// Shifts ligger som JSON i kolonnen "shifts".
    /// </summary>
    [Table("ShiftPlans")]
    public class ShiftPlanRow
    {
        [Key]
        [Column("shiftPlanId")]
        public Guid ShiftPlanId { get; set; } = Guid.NewGuid();

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("startDate")]
        public DateTime StartDate { get; set; }

        [Column("endDate")]
        public DateTime EndDate { get; set; }

        // JSON fra DB — fx en serialized liste af shifts
        [Column("shifts")]
        public string ShiftsJson { get; set; } = "[]";
    }
}
