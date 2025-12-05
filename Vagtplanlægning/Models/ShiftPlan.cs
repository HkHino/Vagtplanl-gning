using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Vagtplanlægning.Models
{
    [BsonIgnoreExtraElements] // Ignorér fx _id og andre felter, vi ikke har properties til
    public class ShiftPlan
    {
        // Almindelig string – IKKE BsonId
        public string ShiftPlanId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<Shift>? Shifts { get; set; } = new();
    }
}
