using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers.AdminControllers;

[ApiController]
   [Route("api/[controller]")]
public class AdminController : BaseAdminController
{
    public AdminController(AppDbContext db, IMapper mapper) : base(db, mapper)
    {
    }

    [HttpGet]
    [Route("check-if-admin")]
    public async Task<IActionResult> CheckIfAdmin()
    {
        return Ok("Admin Verified");
    }
}