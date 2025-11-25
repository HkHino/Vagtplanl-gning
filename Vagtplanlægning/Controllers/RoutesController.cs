using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly AppDbContext _db;
    public RoutesController(AppDbContext db) => _db = db;

    // GET: api/routes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteDto>>> GetAll()
    {
        try
        {
            var routes = await _db.Routes
                .OrderBy(r => r.RouteNumber)
                .Select(r => new RouteDto
                {
                    Id = r.Id,
                    RouteNumber = r.RouteNumber
                })
                .ToListAsync();

            return Ok(routes);
        }
        catch (MySqlConnector.MySqlException ex)
        {
            return StatusCode(500, new
            {
                error = "Database error while reading routes.",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Unexpected server error while reading routes.",
                details = ex.Message
            });
        }
    }


    // GET: api/routes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RouteDto>> GetById(int id)
    {
        var route = await _db.Routes
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RouteDto
            {
                Id = r.Id,
                RouteNumber = r.RouteNumber
            })
            .FirstOrDefaultAsync();

        if (route == null)
        {
            return NotFound(new { error = $"Route with id {id} not found." });
        }

        return Ok(route);
    }

    // POST: api/routes
    [HttpPost]
    public async Task<ActionResult<RouteDto>> Create([FromBody] CreateRouteDto dto)
    {
        var entity = new RouteEntity
        {
            // Id sættes af databasen (AUTO_INCREMENT)
            RouteNumber = dto.RouteNumber
        };

        _db.Routes.Add(entity);
        await _db.SaveChangesAsync();

        var result = new RouteDto
        {
            Id = entity.Id,
            RouteNumber = entity.RouteNumber
        };

        // Vi peger på GetById, fordi den faktisk bruger id'et
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result
        );
    }

    // PUT: api/routes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRouteDto dto)
    {
        var entity = await _db.Routes.FindAsync(id);
        if (entity == null)
        {
            return NotFound(new { error = $"Route with id {id} not found." });
        }

        entity.RouteNumber = dto.RouteNumber;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/routes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Routes.FindAsync(id);
        if (entity == null)
        {
            return NotFound(new { error = $"Route with id {id} not found." });
        }

        _db.Routes.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
