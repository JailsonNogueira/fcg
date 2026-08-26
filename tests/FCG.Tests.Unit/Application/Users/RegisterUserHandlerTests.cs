using FCG.Application.Common;
using FCG.Application.Users.RegisterUser;
using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Unit.Application.Users;

public sealed class RegisterUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldPersistNewPlayer()
    {
        var repository = new InMemoryUserRepository();
        var handler = new RegisterUserHandler(repository, new StubPasswordHasher(), new RecordingUnitOfWork());

        var id = await handler.HandleAsync(new RegisterUserCommand("Alice", "alice@test.com", "Senha@123"));

        Assert.NotEqual(Guid.Empty, id);
        var created = Assert.Single(repository.Items);
        Assert.Equal("alice@test.com", created.Email.Value);
        Assert.Equal(UserRole.Player, created.Role);
    }

    [Fact]
    public async Task HandleAsync_ShouldPersistAdministratorWhenRequested()
    {
        var repository = new InMemoryUserRepository();
        var handler = new RegisterUserHandler(repository, new StubPasswordHasher(), new RecordingUnitOfWork());

        var command = new RegisterUserCommand("Root", "root@test.com", "Senha@123", UserRole.Administrator);

        await handler.HandleAsync(command);

        Assert.Equal(UserRole.Administrator, Assert.Single(repository.Items).Role);
    }

    [Fact]
    public async Task HandleAsync_ShouldHashPasswordBeforePersisting()
    {
        var repository = new InMemoryUserRepository();
        var handler = new RegisterUserHandler(repository, new StubPasswordHasher(), new RecordingUnitOfWork());

        await handler.HandleAsync(new RegisterUserCommand("Alice", "alice@test.com", "Senha@123"));

        Assert.Equal("hashed:Senha@123", Assert.Single(repository.Items).PasswordHash);
    }

    [Fact]
    public async Task HandleAsync_ShouldSaveChanges()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new RegisterUserHandler(new InMemoryUserRepository(), new StubPasswordHasher(), unitOfWork);

        await handler.HandleAsync(new RegisterUserCommand("Alice", "alice@test.com", "Senha@123"));

        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectDuplicateEmail()
    {
        var existing = User.CreatePlayer("Alice", Email.Create("alice@test.com"), "hash");
        var handler = new RegisterUserHandler(
            new InMemoryUserRepository().Seed(existing),
            new StubPasswordHasher(),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new RegisterUserCommand("Alice", "alice@test.com", "Senha@123")));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectWeakPassword()
    {
        var handler = new RegisterUserHandler(
            new InMemoryUserRepository(),
            new StubPasswordHasher(),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new RegisterUserCommand("Alice", "alice@test.com", "12345")));
    }
}
