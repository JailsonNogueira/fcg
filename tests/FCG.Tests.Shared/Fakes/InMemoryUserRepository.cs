using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Shared.Fakes;

public sealed class InMemoryUserRepository : IUserRepository
{
    public List<User> Items { get; } = [];

    public List<User> Updated { get; } = [];

    public InMemoryUserRepository Seed(params User[] users)
    {
        Items.AddRange(users);
        return this;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.SingleOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.SingleOrDefault(u => u.Email.Equals(email)));

    public Task<bool> ExistsByEmailAsync(
        Email email,
        Guid? ignoredUserId = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(u => u.Email.Equals(email) && u.Id != ignoredUserId));

    public Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Count(u => u.Role == UserRole.Administrator && u.IsActive));

    public Task<IReadOnlyCollection<User>> SearchAsync(
        UserRole? role,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<User>>(
            Filter(role, includeInactive).OrderBy(u => u.Name).Skip(skip).Take(take).ToList());

    public Task<int> CountAsync(
        UserRole? role,
        bool includeInactive,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Filter(role, includeInactive).Count());

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Items.Add(user);
        return Task.CompletedTask;
    }

    public void Update(User user) => Updated.Add(user);

    private IEnumerable<User> Filter(UserRole? role, bool includeInactive)
        => Items.Where(u => (!role.HasValue || u.Role == role.Value) && (includeInactive || u.IsActive));
}
