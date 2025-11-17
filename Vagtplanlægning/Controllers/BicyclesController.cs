using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BicyclesController : ControllerBase
{
    private readonly AppDbContext _db;
    public BicyclesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bicycle>>> GetAll() =>
        Ok(await _db.Bicycles.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<ActionResult<Bicycle>> Create(Bicycle bicycle)
    {
        _db.Bicycles.Add(bicycle);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = bicycle.BicycleId }, bicycle);
    }

   [HttpPut("{id:int}/status")]
public async Task<IActionResult> UpdateStatus(int id, [FromQuery] bool inOperate)
{
    var bicycle = await _db.Bicycles.FindAsync(id);
    if (bicycle == null)
        return NotFound();

    bicycle.InOperate = inOperate;
    await _db.SaveChangesAsync();

    return NoContent();
}

}
