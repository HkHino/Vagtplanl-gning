using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [Table("Days")]
    public class DayEntity
    {
        [Column("dayId")] public int DayId { get; set; }
        [Column("day")] public DateTime Day { get; set; }
    }
}
