using Microsoft.AspNetCore.Authorization;

namespace Vagtplanlægning.Authentication.Policies;

public class IsUserRequirement : IAuthorizationRequirement
{
    public string RoleOfUser { get; }

    public IsUserRequirement(string roleOfUser)
    {
        RoleOfUser = roleOfUser;
    }
}

public class IsUserHandler : AuthorizationHandler<IsUserRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsUserRequirement requirement)
    {
        // TODO: No reason for admin to do this?
        if (context.User.IsInRole(requirement.RoleOfUser) || context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}