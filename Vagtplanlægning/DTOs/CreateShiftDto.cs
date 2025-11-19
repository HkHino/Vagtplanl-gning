namespace Vagtplanlægning.DTOs
{
    public class CreateShiftDto
    {
        public DateTime DateOfShift { get; set; }
        public int EmployeeId { get; set; }
        public int BicycleId { get; set; }
        public int RouteId { get; set; }
        public int SubstitutedId { get; set; }
    }
}
