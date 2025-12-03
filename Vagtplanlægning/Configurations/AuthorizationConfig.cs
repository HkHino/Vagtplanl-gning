using Microsoft.AspNetCore.Authentication.JwtBearer;
using Vagtplanlægning.Authentication.Policies;

namespace Vagtplanlægning.Configurations;

public static class AuthorizationConfig
{
    public static void Configure(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("IsAdmin", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new IsAdminRequirement("Admin"));
                });
            options.AddPolicy("IsUser", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new IsUserRequirement("User"));
                });
        });
    }
    
}