using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly IRouteRepository _routeRepo;
    private readonly ILogger<RoutesController> _logger;
    public RoutesController(IRouteRepository routeRepo,ILogger<RoutesController> logger)
        {
            _routeRepo = routeRepo;
            _logger = logger;
        }

    // GET: api/routes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteDto>>> GetAll(CancellationToken ct)
    {
        try
        {
            var entities = await _routeRepo.GetAllAsync(ct);

            var result = entities
                .Select(r => new RouteDto
                {
                    Id = r.Id,
                    RouteNumber = r.RouteNumber
                })
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while reading routes.");
            return StatusCode(500, new { error = "Unexpected server error while reading routes." });
        }
    }



    // GET: api/routes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RouteDto>> GetById(int id, CancellationToken ct)
    {
        try
        {
            var entity = await _routeRepo.GetByIdAsync(id, ct);

            if (entity == null)
            {
                return NotFound(new { error = $"Route with id {id} not found." });
            }

            var dto = new RouteDto
            {
                Id = entity.Id,
                RouteNumber = entity.RouteNumber
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while reading route {RouteId}.", id);
            return StatusCode(500, new { error = "Unexpected server error while reading route." });
        }
    }

    // POST: api/routes
    [HttpPost]
    public async Task<ActionResult<RouteDto>> Create([FromBody] CreateRouteDto dto, CancellationToken ct)
    {
        try
        {
            var entity = new RouteEntity
            {
                // Id sættes af databasen eller Mongo-repoet
                RouteNumber = dto.RouteNumber
            };

            await _routeRepo.AddAsync(entity, ct);

            var result = new RouteDto
            {
                Id = entity.Id,
                RouteNumber = entity.RouteNumber
            };

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating route.");
            return StatusCode(500, new { error = "Unexpected server error while creating route." });
        }
    }

    // PUT: api/routes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRouteDto dto, CancellationToken ct)
    {
        try
        {
            var entity = await _routeRepo.GetByIdAsync(id, ct);
            if (entity == null)
            {
                return NotFound(new { error = $"Route with id {id} not found." });
            }

            entity.RouteNumber = dto.RouteNumber;

            await _routeRepo.UpdateAsync(entity, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating route {RouteId}.", id);
            return StatusCode(500, new { error = "Unexpected server error while updating route." });
        }
    }

    // DELETE: api/routes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _routeRepo.DeleteAsync(id, ct);
            if (!deleted)
            {
                return NotFound(new { error = $"Route with id {id} not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting route {RouteId}.", id);
            return StatusCode(500, new { error = "Unexpected server error while deleting route." });
        }
    }
}
