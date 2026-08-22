using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Application.Users.RegisterUser;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

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
        Assert.Single(repository.AddedUsers);
        Assert.Equal("alice@test.com", repository.AddedUsers[0].Email.Value);
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
        var handler = new RegisterUserHandler(
            new InMemoryUserRepository { EmailExists = true },
            new StubPasswordHasher(),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new RegisterUserCommand("Alice", "alice@test.com", "Senha@123")));
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public bool EmailExists { get; init; }
        public List<User> AddedUsers { get; } = [];

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
            Task.FromResult(EmailExists);

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            AddedUsers.Add(user);
            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(null);

        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(null);

        public Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public void Update(User user) { }
    }

    private sealed class StubPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public bool WasSaved { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            WasSaved = true;
            return Task.CompletedTask;
        }
    }
}
