using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Vagtplanlægning.Authentication;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers;


[Authorize(Policy = "IsEmployee")]
public class BaseEmployeeController : BaseController
{
    
    // This I took from my internship so can't explain it
    public BaseEmployeeController(AppDbContext db, IMapper mapper) : base(db, mapper)
    {
    }

    protected UserPrincipal UserPrincipal
    {
        get
        {
            var currentlyLoggedUser = JwtHelper.GetUser(this.User);
            return currentlyLoggedUser;
        }
    }




}