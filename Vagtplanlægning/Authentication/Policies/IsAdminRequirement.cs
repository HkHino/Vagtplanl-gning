using Microsoft.AspNetCore.Authorization;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Authentication.Policies;

// Here we make the requirements for the authorization
public class IsAdminRequirement : IAuthorizationRequirement
{
    public UserRole RoleOfAdmin { get; }

    public IsAdminRequirement(UserRole roleOfAdmin)
    {
        RoleOfAdmin = roleOfAdmin;
    }
    
}

// Function for the process of authorizing
public class IsAdminHandler : AuthorizationHandler<IsAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdminRequirement requirement)
    {

        if (context.User.IsInRole(requirement.RoleOfAdmin.ToString()))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
        return Task.CompletedTask;
    }
}