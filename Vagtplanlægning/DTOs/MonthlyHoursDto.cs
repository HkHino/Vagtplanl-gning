namespace Vagtplanlægning.DTOs
{
    public class MonthlyHoursRow
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalMonthlyHours { get; set; }
        public bool HasSubstituted { get; set; }
    }
}
