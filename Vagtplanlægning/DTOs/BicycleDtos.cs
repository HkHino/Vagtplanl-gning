namespace Vagtplanlægning.DTOs
{
    /// <summary>
    /// Simpelt response-DTO for en cykel.
    /// Det er det, vi viser i Swagger/til frontend.
    /// </summary>
    public class BicycleDto
    {
        public int BicycleId { get; set; }
        public int BicycleNumber { get; set; }
        public bool InOperate { get; set; }
    }

    /// <summary>
    /// Request-DTO til at oprette en cykel.
    /// Bemærk: intet BicycleId – det genereres af databasen.
    /// </summary>
    public class CreateBicycleDto
    {
        public int BicycleNumber { get; set; }
        public bool InOperate { get; set; } = true;
    }
}
