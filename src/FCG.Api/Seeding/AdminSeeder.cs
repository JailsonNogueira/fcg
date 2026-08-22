using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Infrastructure.Persistence;
using FCG.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FCG.Api.Seeding;

public sealed class AdminSeeder(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<AdminSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<BCryptPasswordHasher>();

        await context.Database.MigrateAsync(cancellationToken);

        var adminCount = await userRepository.CountActiveAdministratorsAsync(cancellationToken);
        if (adminCount > 0)
        {
            logger.LogInformation("AdminSeeder: administrador já existe, nenhuma ação necessária.");
            return;
        }

        var name = configuration["AdminSeed:Name"] ?? "Administrador";
        var emailStr = configuration["AdminSeed:Email"] ?? "admin@fcg.com";
        var passwordStr = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(passwordStr))
        {
            logger.LogWarning("AdminSeeder: AdminSeed:Password não configurada. Seeder ignorado.");
            return;
        }

        var email = Email.Create(emailStr);
        var password = Password.Create(passwordStr);
        var passwordHash = hasher.Hash(password);
        var admin = User.CreateAdministrator(name, email, passwordHash);

        await userRepository.AddAsync(admin, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("AdminSeeder: administrador criado com e-mail {Email}.", emailStr);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
