using Microsoft.AspNetCore.Authorization;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Authentication.Policies;

public class IsEmployeeRequirement : IAuthorizationRequirement
{
    public UserRole RoleOfEmployee { get; }

    public IsEmployeeRequirement(UserRole roleOfEmployee)
    {
        RoleOfEmployee = roleOfEmployee;
    }
}

public class IsUserHandler : AuthorizationHandler<IsEmployeeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsEmployeeRequirement requirement)
    {
        // Checks if user is an Employee. Also will let in users with the admin role
        if (context.User.IsInRole(nameof(requirement.RoleOfEmployee)) || context.User.IsInRole(nameof(UserRole.Admin)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}