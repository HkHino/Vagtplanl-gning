using System;
using System.Collections.Generic;

namespace Vagtplanlægning.DTOs
{
    // ----------------------------------------------------------
    // REQUEST: Bruges af POST /api/shiftplans/generate
    // ----------------------------------------------------------
    public class GenerateShiftPlanRequestDto
    {
        public DateTime StartDate { get; set; }
        public int Weeks { get; set; } = 6; // default = 6 uger
    }

    // ----------------------------------------------------------
    // RESPONSE: Bruges af GET /api/shiftplans
    // ----------------------------------------------------------
    public class ShiftPlanSummaryDto
    {
        public string ShiftPlanId { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ShiftCount { get; set; }
    }

    // ----------------------------------------------------------
    // RESPONSE: Bruges af GET /api/shiftplans/{id}
    // ----------------------------------------------------------
    public class ShiftPlanDetailDto
    {
        public string ShiftPlanId { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<ShiftInPlanDto> Shifts { get; set; } = new();
    }

    // ----------------------------------------------------------
    // Et shift inde i en plan — kun det nødvendige.
    // ----------------------------------------------------------
    public class ShiftInPlanDto
    {
        public int ShiftId { get; set; }

        public DateTime DateOfShift { get; set; }

        public int EmployeeId { get; set; }

        public int RouteId { get; set; }

        public int BicycleId { get; set; }

        public int SubstitutedId { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public decimal? TotalHours { get; set; }
    }
    public class UpdateShiftInPlanDto
    {
        public DateTime DateOfShift { get; set; }
        public int EmployeeId { get; set; }
        public int BicycleId { get; set; }
        public int RouteId { get; set; }
        public int SubstitutedId { get; set; }
    }

}
