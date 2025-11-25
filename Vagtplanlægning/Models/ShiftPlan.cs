using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [NotMapped] 
    public class ShiftPlan
    {
        public string ShiftPlanId { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [NotMapped] // just to doubly guarantee EF ignores it
        public List<Shift> Shifts { get; set; } = new();
    }
}
