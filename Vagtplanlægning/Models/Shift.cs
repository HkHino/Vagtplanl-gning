using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægning.Models
{
    [Table("ListOfShift")]
    public class Shift
    {
        [Column("shiftId")] public int ShiftId { get; set; }
        [Column("dayId")] public int DayId { get; set; }
        [Column("employeeId")] public int EmployeeId { get; set; }
        [Column("bicycleId")] public int BicycleId { get; set; }
        [Column("substitutedId")] public int SubstitutedId { get; set; } // NOT NULL i DB
        [Column("routeNumberId")] public int RouteNumberId { get; set; }
        [Column("meetInTime")] public TimeSpan MeetInTime { get; set; }
        [Column("startTime")] public TimeSpan? StartTime { get; set; }
        [Column("endTime")] public TimeSpan? EndTime { get; set; }
        [Column("totalHours")] public decimal? TotalHours { get; set; }
    }
}
