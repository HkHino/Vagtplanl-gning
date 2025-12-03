using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers;



public class BaseController : ControllerBase
{
    protected readonly AppDbContext _db;

    public BaseController(AppDbContext db)
    {
        _db = db;
    }
    
}