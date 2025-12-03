using Microsoft.AspNetCore.Authorization;
using Vagtplanlægning.Authentication;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers;

// Checks if the user's token contains Admin role
[Authorize(Policy = "IsAdmin")]
public class BaseAdminController : BaseController
{
    public BaseAdminController(AppDbContext _db) : base(_db)
    {
        
    }
    
}