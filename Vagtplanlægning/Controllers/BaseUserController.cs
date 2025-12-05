using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Vagtplanlægning.Authentication;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers;


[Authorize(Policy = "IsUser")]
public class BaseUserController : BaseController
{
    
    // This I took from my internship so can't explain it
    protected UserPrincipal UserPrincipal
    {
        get
        {
            var currentlyLoggedUser = JwtHelper.GetUser(this.User);
            return currentlyLoggedUser;
        }
    }

    public BaseUserController(AppDbContext _db) : base(_db)
    {
    }


}