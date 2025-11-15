namespace Vagtplanlægning.Models
{
    // this matches the MySQL table ShiftPlans
    public class ShiftPlanRow
    {
        public string ShiftPlanId { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // this is the JSON column in MySQL
        public string ShiftsJson { get; set; } = "[]";
    }
}
