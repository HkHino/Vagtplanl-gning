using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BicyclesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BicyclesController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/bicycles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BicycleDto>>> GetAll()
        {
            var bikes = await _db.Bicycles
                .AsNoTracking()
                .Select(b => new BicycleDto
                {
                    BicycleId = b.BicycleId,
                    BicycleNumber = b.BicycleNumber,
                    InOperate = b.InOperate
                })
                .ToListAsync();

            return Ok(bikes);
        }

        // GET: api/bicycles/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BicycleDto>> GetById(int id)
        {
            var bike = await _db.Bicycles
                .AsNoTracking()
                .Where(b => b.BicycleId == id)
                .Select(b => new BicycleDto
                {
                    BicycleId = b.BicycleId,
                    BicycleNumber = b.BicycleNumber,
                    InOperate = b.InOperate
                })
                .FirstOrDefaultAsync();

            if (bike == null)
            {
                return NotFound(new { error = $"Bicycle with id {id} not found." });
            }

            return Ok(bike);
        }

        // POST: api/bicycles
        [HttpPost]
        public async Task<ActionResult<BicycleDto>> Create([FromBody] CreateBicycleDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        error = "Request body is missing or invalid."
                    });
                }

                if (dto.BicycleNumber < 0)
                {
                    return BadRequest(new
                    {
                        error = "BicycleNumber cannot be negative."
                    });
                }

                // Autogenerér nummer hvis dto.BicycleNumber == 0
                int bicycleNumber;

                if (dto.BicycleNumber == 0)
                {
                    var maxNumber = await _db.Bicycles
                        .Select(b => (int?)b.BicycleNumber)
                        .MaxAsync();

                    bicycleNumber = (maxNumber ?? 0) + 1;
                }
                else
                {
                    // Tjek om nummer allerede findes
                    bool exists = await _db.Bicycles
                        .AnyAsync(b => b.BicycleNumber == dto.BicycleNumber);

                    if (exists)
                    {
                        return Conflict(new
                        {
                            error = $"BicycleNumber {dto.BicycleNumber} already exists."
                        });
                    }

                    bicycleNumber = dto.BicycleNumber;
                }

                var entity = new Bicycle
                {
                    BicycleNumber = bicycleNumber,
                    InOperate = dto.InOperate
                };

                _db.Bicycles.Add(entity);
                await _db.SaveChangesAsync();

                var result = new BicycleDto
                {
                    BicycleId = entity.BicycleId,
                    BicycleNumber = entity.BicycleNumber,
                    InOperate = entity.InOperate
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.BicycleId },
                    result);
            }
            catch (MySqlConnector.MySqlException ex) when (ex.ErrorCode == MySqlConnector.MySqlErrorCode.DuplicateKeyEntry)
            {
                // MySQLs egen duplicate key fejl (sikkerhedsnet)
                return Conflict(new
                {
                    error = "A bicycle with this number already exists.",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Alt andet — 500
                return StatusCode(500, new
                {
                    error = "Unexpected server error. Contact support.",
                    details = ex.Message
                });
            }
        }


        // PUT: api/bicycles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateBicycleDto dto)
        {
            var entity = await _db.Bicycles.FindAsync(id);
            if (entity == null)
            {
                return NotFound(new { error = $"Bicycle with id {id} not found." });
            }

            entity.BicycleNumber = dto.BicycleNumber;
            entity.InOperate = dto.InOperate;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/bicycles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Bicycles.FindAsync(id);
            if (entity == null)
            {
                return NotFound(new { error = $"Bicycle with id {id} not found." });
            }

            _db.Bicycles.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
