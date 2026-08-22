using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(FcgDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Set<User>().FindAsync([id], cancellationToken);

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => await context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => await context.Set<User>()
            .AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default)
        => await context.Set<User>()
            .CountAsync(u => u.Role == UserRole.Administrator && u.IsActive, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await context.Set<User>().AddAsync(user, cancellationToken);

    public void Update(User user)
        => context.Set<User>().Update(user);
}
