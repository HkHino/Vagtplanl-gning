namespace Vagtplanlægning.DTOs
{
    public class RouteDto
    {
        public int Id { get; set; }
        public int RouteNumber { get; set; }
    }

    public class CreateRouteDto
    {
        public int RouteNumber { get; set; }
    }
}
