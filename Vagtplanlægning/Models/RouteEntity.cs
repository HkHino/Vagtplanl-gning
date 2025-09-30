using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [Table("Route")]
    public class RouteEntity
    {
        [Column("routeNumberId")] public int RouteNumberId { get; set; }
    }
}
