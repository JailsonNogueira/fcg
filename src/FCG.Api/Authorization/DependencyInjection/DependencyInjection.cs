using FCG.Domain.Users.Enums;

namespace FCG.Api.Authorization.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddFcgAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.ManageCatalog, policy =>
            {
                policy.RequireRole(nameof(UserRole.Administrator));
            });
            options.AddPolicy(Policies.ManagePromotions, policy =>
            {
                policy.RequireRole(nameof(UserRole.Administrator));
            });
            options.AddPolicy(Policies.ManageUsers, policy =>
            {
                policy.RequireRole(nameof(UserRole.Administrator));
            });
            options.AddPolicy(Policies.Library, policy =>
            {
                policy.RequireRole(nameof(UserRole.Player));
                policy.RequireRole(nameof(UserRole.Administrator));
            });
        });
        return services;
    }
}
