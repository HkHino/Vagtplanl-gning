using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers.AdminControllers;

[ApiController]
[Route("api/[controller]")]
public class MigrateController : BaseAdminController
{
    public MigrateController(AppDbContext db, IMapper mapper) : base(db, mapper)
    {
    }
    
}