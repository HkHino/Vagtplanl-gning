using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BicyclesController : ControllerBase
    {
        private readonly IBicycleRepository _bicycleRepo;

        public BicyclesController(IBicycleRepository bicycleRepo)
        {
            _bicycleRepo = bicycleRepo;
        }
        
        // GET: /api/Bicycles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BicycleDto>>> GetAll(CancellationToken ct)
        {
            var bicycles = await _bicycleRepo.GetAllAsync(ct);

            var result = bicycles.Select(b => new BicycleDto
            {
                BicycleId = b.BicycleId,
                BicycleNumber = b.BicycleNumber,
                InOperate = b.InOperate
            });

            return Ok(result);
        }

        // GET: /api/Bicycles/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BicycleDto>> GetById(int id, CancellationToken ct)
        {
            var b = await _bicycleRepo.GetByIdAsync(id, ct);
            if (b == null)
                return NotFound(new { error = $"Bicycle with id {id} not found." });

            return Ok(new BicycleDto
            {
                BicycleId = b.BicycleId,
                BicycleNumber = b.BicycleNumber,
                InOperate = b.InOperate
            });
        }

        // POST: /api/Bicycles
        [HttpPost]
        public async Task<ActionResult<BicycleDto>> Create(CreateBicycleDto dto, CancellationToken ct)
        {
            var entity = new Bicycle
            {
                BicycleNumber = dto.BicycleNumber,
                InOperate = dto.InOperate
            };

            await _bicycleRepo.AddAsync(entity, ct);

            var result = new BicycleDto
            {
                BicycleId = entity.BicycleId,
                BicycleNumber = entity.BicycleNumber,
                InOperate = entity.InOperate
            };

            return CreatedAtAction(nameof(GetById), new { id = entity.BicycleId }, result);
        }

        // PUT: /api/Bicycles/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateBicycleDto dto, CancellationToken ct)
        {
            var existing = await _bicycleRepo.GetByIdAsync(id, ct);
            if (existing == null)
                return NotFound(new { error = $"Bicycle with id {id} not found." });

            existing.BicycleNumber = dto.BicycleNumber;
            existing.InOperate = dto.InOperate;

            await _bicycleRepo.UpdateAsync(existing, ct);

            return NoContent();
        }

        // DELETE: /api/Bicycles/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var deleted = await _bicycleRepo.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(new { error = $"Bicycle with id {id} not found." });

            return NoContent();
        }
    }
}
