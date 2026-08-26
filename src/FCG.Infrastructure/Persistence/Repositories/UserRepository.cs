using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(FcgDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Users.FindAsync([id], cancellationToken);

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(
        Email email,
        Guid? ignoredUserId = null,
        CancellationToken cancellationToken = default)
        => await context.Users
            .AnyAsync(u => u.Email == email && (!ignoredUserId.HasValue || u.Id != ignoredUserId), cancellationToken);

    public async Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default)
        => await context.Users
            .CountAsync(u => u.Role == UserRole.Administrator && u.IsActive, cancellationToken);

    public async Task<IReadOnlyCollection<User>> SearchAsync(
        UserRole? role,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
        => await Filter(role, includeInactive)
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<int> CountAsync(
        UserRole? role,
        bool includeInactive,
        CancellationToken cancellationToken = default)
        => await Filter(role, includeInactive).CountAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await context.Users.AddAsync(user, cancellationToken);

    public void Update(User user)
        => context.Users.Update(user);

    private IQueryable<User> Filter(UserRole? role, bool includeInactive)
    {
        var query = context.Users.AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        return query;
    }
}
