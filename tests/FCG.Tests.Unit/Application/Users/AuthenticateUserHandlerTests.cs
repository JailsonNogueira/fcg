using FCG.Application.Common;
using FCG.Application.Users.AuthenticateUser;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Unit.Application.Users;

public sealed class AuthenticateUserHandlerTests
{
    private const string PlainPassword = "Senha@123";

    [Fact]
    public async Task HandleAsync_ShouldReturnTokenForValidCredentials()
    {
        var user = CreateActivePlayer();
        var handler = Build(user);

        var result = await handler.HandleAsync(new AuthenticateUserCommand("player@test.com", PlainPassword));

        Assert.Equal($"token:{user.Id}", result.Token);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("player@test.com", result.Email);
        Assert.Equal("Player", result.Role);
    }

    [Fact]
    public async Task HandleAsync_ShouldNormalizeTheEmailBeforeLookingUpTheAccount()
    {
        var handler = Build(CreateActivePlayer());

        var result = await handler.HandleAsync(new AuthenticateUserCommand("  PLAYER@TEST.COM ", PlainPassword));

        Assert.Equal("player@test.com", result.Email);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectWrongPassword()
    {
        var handler = Build(CreateActivePlayer());

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.HandleAsync(new AuthenticateUserCommand("player@test.com", "OutraSenha@1")));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectUnknownEmail()
    {
        var handler = Build(CreateActivePlayer());

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.HandleAsync(new AuthenticateUserCommand("outro@test.com", PlainPassword)));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectInactiveAccount()
    {
        var user = CreateActivePlayer();
        user.Deactivate();
        var handler = Build(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.HandleAsync(new AuthenticateUserCommand("player@test.com", PlainPassword)));
    }

    [Fact]
    public async Task HandleAsync_ShouldTreatMalformedEmailAsInvalidCredentials()
    {
        var handler = Build(CreateActivePlayer());

        // Um e-mail inválido não pode vazar, via 400, que a conta não existe.
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.HandleAsync(new AuthenticateUserCommand("nao-e-email", PlainPassword)));
    }

    private static User CreateActivePlayer()
        => User.CreatePlayer(
            "Player",
            Email.Create("player@test.com"),
            new StubPasswordHasher().Hash(PlainPassword));

    private static AuthenticateUserHandler Build(User user)
        => new(
            new InMemoryUserRepository().Seed(user),
            new StubPasswordHasher(),
            new StubTokenGenerator());
}
