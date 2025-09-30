using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [Table("Bicycles")]
    public class Bicycle
    {
        [Column("bicycleId")] public int BicycleId { get; set; }
        [Column("inOperate")] public bool InOperate { get; set; }
    }
}
