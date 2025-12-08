namespace Vagtplanlægning.DTOs;

public class ShiftDto
{
    public int ShiftId { get; set; }
    
    public DateTime DateOfShift { get; set; }

    public int SubstitutedId { get; set; }

    public int RouteId { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public decimal? TotalHours { get; set; }
}