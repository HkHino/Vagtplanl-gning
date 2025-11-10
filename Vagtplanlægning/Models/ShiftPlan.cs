namespace Vagtplanlægning.Models
{
    public class ShiftPlan
    {
        public string ShiftPlanId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Shift> Shifts { get; set; } = new();
    }
}
