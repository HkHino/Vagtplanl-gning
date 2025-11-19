using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [Table("Bicycles")]
    public class Bicycle
    {        
        [Column("id")]
        public int BicycleId { get; set; }

        [Column("bicycleNumber")]
        public int BicycleNumber { get; set; }

        [Column("inOperate")]
        public bool InOperate { get; set; }
    }
}
