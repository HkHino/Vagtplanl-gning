using Microsoft.AspNetCore.Authorization;

namespace Vagtplanlægning.Authentication.Policies;

// Here we make the requirements for the authorization
public class IsAdminRequirement : IAuthorizationRequirement
{
    public string RoleOfAdmin { get; }

    public IsAdminRequirement(string roleOfAdmin)
    {
        RoleOfAdmin = roleOfAdmin;
    }
    
}

// Function for the process of authorizing
public class IsAdminHandler : AuthorizationHandler<IsAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdminRequirement requirement)
    {
        if (context.User.IsInRole(requirement.RoleOfAdmin))
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