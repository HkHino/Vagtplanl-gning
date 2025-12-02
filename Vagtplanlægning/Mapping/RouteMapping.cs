using Vagtplanlægning.Models;
using Vagtplanlægning.DTOs;

public static class RouteMapping
{
    public static RouteDto ToDto(this RouteEntity r)
        => new RouteDto
        {
            Id = r.Id,
            RouteNumber = r.RouteNumber
        };

    public static RouteEntity ToEntity(this CreateRouteDto dto)
        => new RouteEntity
        {
            RouteNumber = dto.RouteNumber
        };

    public static void ApplyTo(this UpdateRouteDto dto, RouteEntity r)
    {
        r.RouteNumber = dto.RouteNumber;
    }
}
