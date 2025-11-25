using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    // VIGTIGT: Match præcis tabelnavn i databasen
    [Table("Route")]
    public class RouteEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("routeNumber")]
        public int RouteNumber { get; set; }

        public List<Shift> Shifts { get; set; } = new();
    }
}
