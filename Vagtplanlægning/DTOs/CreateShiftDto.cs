namespace Vagtplanlægning.DTOs
{
    public class CreateShiftDto
    {
        public DateTime Day { get; set; }        
        public int EmployeeId { get; set; }
        public int BicycleId { get; set; }
        public int RouteNumberId { get; set; }
        public TimeSpan MeetInTime { get; set; }
        public int? SubstitutedId { get; set; }    
    }
}
