using FCG.Application.Abstractions;
using FCG.Domain.Users;

namespace FCG.Tests.Shared.Fakes;

public sealed class FixedClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset UtcNow => now;
}

public sealed class RecordingUnitOfWork : IUnitOfWork
{
    public bool WasSaved { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        WasSaved = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Hash previsível: mantém os testes independentes do custo do BCrypt.
/// </summary>
public sealed class StubPasswordHasher : IPasswordHasher
{
    private const string Prefix = "hashed:";

    public string Hash(string password) => $"{Prefix}{password}";

    public bool Verify(string password, string passwordHash)
        => passwordHash == $"{Prefix}{password}";
}

public sealed class StubTokenGenerator : ITokenGenerator
{
    public string Generate(User user) => $"token:{user.Id}";
}
