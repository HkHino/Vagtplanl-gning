using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly AppDbContext _db;
    public RoutesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteEntity>>> GetAll() =>
        Ok(await _db.Routes.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<ActionResult<RouteEntity>> Create(RouteEntity route)
    {
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = route.RouteNumberId }, route);
    }
}
