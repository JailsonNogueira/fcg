using FCG.Application.Common;
using FCG.Application.Users.ActivateUser;
using FCG.Application.Users.DeactivateUser;
using FCG.Application.Users.GetUserById;
using FCG.Application.Users.GetUsers;
using FCG.Application.Users.UpdateUser;
using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Unit.Application.Users;

public sealed class ManageUsersHandlerTests
{
    [Fact]
    public async Task GetUsers_ShouldHideInactiveAccountsByDefault()
    {
        var inactive = Player("Bruno", "bruno@test.com");
        inactive.Deactivate();
        var repository = new InMemoryUserRepository().Seed(Player("Alice", "alice@test.com"), inactive);

        var result = await new GetUsersHandler(repository).HandleAsync(new GetUsersQuery());

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Alice", Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task GetUsers_ShouldFilterByRole()
    {
        var repository = new InMemoryUserRepository().Seed(
            Player("Alice", "alice@test.com"),
            User.CreateAdministrator("Root", Email.Create("root@test.com"), "hash"));

        var result = await new GetUsersHandler(repository)
            .HandleAsync(new GetUsersQuery(Role: UserRole.Administrator));

        Assert.Equal("Administrator", Assert.Single(result.Items).Role);
    }

    [Fact]
    public async Task GetUsers_ShouldPageTheResult()
    {
        var repository = new InMemoryUserRepository().Seed(
            Player("Alice", "alice@test.com"),
            Player("Bruno", "bruno@test.com"),
            Player("Carla", "carla@test.com"));

        var result = await new GetUsersHandler(repository)
            .HandleAsync(new GetUsersQuery(Page: 2, PageSize: 2));

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("Carla", Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task GetUserById_ShouldRejectUnknownUser()
    {
        var handler = new GetUserByIdHandler(new InMemoryUserRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new GetUserByIdQuery(Guid.NewGuid())));
    }

    [Fact]
    public async Task UpdateUser_ShouldChangeNameAndEmail()
    {
        var user = Player("Alice", "alice@test.com");
        var repository = new InMemoryUserRepository().Seed(user);
        var unitOfWork = new RecordingUnitOfWork();

        await new UpdateUserHandler(repository, unitOfWork)
            .HandleAsync(new UpdateUserCommand(user.Id, "Alice Souza", "alice.souza@test.com"));

        Assert.Equal("Alice Souza", user.Name);
        Assert.Equal("alice.souza@test.com", user.Email.Value);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task UpdateUser_ShouldAcceptTheAccountsOwnEmail()
    {
        var user = Player("Alice", "alice@test.com");
        var repository = new InMemoryUserRepository().Seed(user);

        await new UpdateUserHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new UpdateUserCommand(user.Id, "Alice Souza", "alice@test.com"));

        Assert.Equal("Alice Souza", user.Name);
    }

    [Fact]
    public async Task UpdateUser_ShouldRejectEmailOwnedByAnotherAccount()
    {
        var user = Player("Alice", "alice@test.com");
        var repository = new InMemoryUserRepository().Seed(user, Player("Bruno", "bruno@test.com"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            new UpdateUserHandler(repository, new RecordingUnitOfWork())
                .HandleAsync(new UpdateUserCommand(user.Id, "Alice", "bruno@test.com")));
    }

    [Fact]
    public async Task DeactivateUser_ShouldInactivateThePlayer()
    {
        var user = Player("Alice", "alice@test.com");
        var repository = new InMemoryUserRepository().Seed(user);

        await new DeactivateUserHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new DeactivateUserCommand(user.Id));

        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task DeactivateUser_ShouldRejectRemovingTheLastActiveAdministrator()
    {
        var admin = User.CreateAdministrator("Root", Email.Create("root@test.com"), "hash");
        var repository = new InMemoryUserRepository().Seed(admin, Player("Alice", "alice@test.com"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            new DeactivateUserHandler(repository, new RecordingUnitOfWork())
                .HandleAsync(new DeactivateUserCommand(admin.Id)));

        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task DeactivateUser_ShouldAllowRemovingAnAdministratorWhenAnotherRemains()
    {
        var admin = User.CreateAdministrator("Root", Email.Create("root@test.com"), "hash");
        var backup = User.CreateAdministrator("Backup", Email.Create("backup@test.com"), "hash");
        var repository = new InMemoryUserRepository().Seed(admin, backup);

        await new DeactivateUserHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new DeactivateUserCommand(admin.Id));

        Assert.False(admin.IsActive);
    }

    [Fact]
    public async Task ActivateUser_ShouldRestoreAnInactiveAccount()
    {
        var user = Player("Alice", "alice@test.com");
        user.Deactivate();
        var repository = new InMemoryUserRepository().Seed(user);

        await new ActivateUserHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new ActivateUserCommand(user.Id));

        Assert.True(user.IsActive);
    }

    private static User Player(string name, string email)
        => User.CreatePlayer(name, Email.Create(email), "hash");
}
