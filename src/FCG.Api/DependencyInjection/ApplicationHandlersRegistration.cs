using FCG.Application.Users.RegisterUser;

namespace FCG.Api.DependencyInjection;

public static class ApplicationHandlersRegistration
{
    /// <summary>
    /// Registra por convenção todos os handlers da camada de aplicação, evitando que um
    /// caso de uso novo suba com o container incompleto.
    /// </summary>
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        var handlers = typeof(RegisterUserHandler).Assembly
            .GetExportedTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                && type.Name.EndsWith("Handler", StringComparison.Ordinal));

        foreach (var handler in handlers)
        {
            services.AddScoped(handler);
        }

        return services;
    }
}
