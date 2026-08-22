using FCG.Application.Abstractions;
using FCG.Domain.Games;
using FCG.Domain.Libraries;
using FCG.Domain.Promotions;
using FCG.Domain.Users;
using FCG.Infrastructure.Persistence;
using FCG.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FcgDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<ILibraryItemRepository, LibraryItemRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        return services;
    }
}
