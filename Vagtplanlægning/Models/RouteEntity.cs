using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [Table("Route")]
    public class RouteEntity
    {        
        [Column("id")]
        public int RouteNumberId { get; set; }

        [Column("routeNumber")]
        public int RouteNumber { get; set; }
    }
}
