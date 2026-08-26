using FCG.Domain.Users.Enums;

namespace FCG.Api.Authorization.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddFcgAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.ManageCatalog, policy =>
                policy.RequireRole(nameof(UserRole.Administrator)));

            options.AddPolicy(Policies.ManagePromotions, policy =>
                policy.RequireRole(nameof(UserRole.Administrator)));

            options.AddPolicy(Policies.ManageUsers, policy =>
                policy.RequireRole(nameof(UserRole.Administrator)));

            options.AddPolicy(Policies.Catalog, policy =>
                policy.RequireRole(nameof(UserRole.Player), nameof(UserRole.Administrator)));

            // Um único RequireRole com vários papéis é OR. Duas chamadas separadas seriam AND,
            // e nenhuma conta possui os dois papéis ao mesmo tempo.
            options.AddPolicy(Policies.Library, policy =>
                policy.RequireRole(nameof(UserRole.Player), nameof(UserRole.Administrator)));
        });

        return services;
    }
}
