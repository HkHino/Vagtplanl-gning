using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftPlansController : ControllerBase
{
    private readonly IShiftPlanService _planService;
    private readonly IShiftPlanRepository _planRepo;

    public ShiftPlansController(IShiftPlanService planService, IShiftPlanRepository planRepo)
    {
        _planService = planService;
        _planRepo = planRepo;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ShiftPlan>> Generate([FromQuery] DateTime? startDate)
    {
        var date = startDate ?? DateTime.Today;
        var plan = await _planService.Generate6WeekPlanAsync(date);
        return Ok(plan);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftPlan>>> GetAll()
    {
        var plans = await _planRepo.GetAllAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShiftPlan>> GetById(string id)
    {
        var plan = await _planRepo.GetByIdAsync(id);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var ok = await _planRepo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
