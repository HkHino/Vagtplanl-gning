using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Vagtplanlægning.Authentication;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers;
/// <summary>
/// Base controller for all employee-facing controllers.
///
/// Responsibilities:
/// - Enforces the "IsEmployee" authorization policy.
/// - Exposes <see cref="UserPrincipal"/> so derived controllers can easily
///   access information about the currently logged-in user from the JWT.
/// - Inherits from <see cref="BaseController"/> to get access to <see cref="AppDbContext"/>
///   and AutoMapper.
/// </summary>

[Authorize(Policy = "IsEmployee")]
public class BaseEmployeeController : BaseController
{
    // <summary>
    /// Creates a new <see cref="BaseEmployeeController"/>.
    /// </summary>
    /// <param name="db">The EF Core database context.</param>
    /// <param name="mapper">AutoMapper instance used for mapping entities to DTOs and vice versa.</param>
        
    public BaseEmployeeController(AppDbContext db, IMapper mapper) : base(db, mapper)
    {
    }
    /// <summary>
    /// Gets the current user as a <see cref="UserPrincipal"/>, extracted from the JWT.
    ///
    /// The <see cref="JwtHelper"/> reads the <see cref="System.Security.Claims.ClaimsPrincipal"/>
    /// on <see cref="Microsoft.AspNetCore.Mvc.ControllerBase.User"/> and converts it into a
    /// strongly-typed object with the user id and roles.
    /// </summary>

    protected UserPrincipal UserPrincipal
    {
        get
        {
            var currentlyLoggedUser = JwtHelper.GetUser(this.User);
            return currentlyLoggedUser;
        }
    }
}