using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace FCG.Infrastructure.Persistence.Repositories;
public sealed class UserRepository(FcgDbContext context) : IUserRepository { public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => context.Users.SingleOrDefaultAsync(x => x.Id == id, ct); public Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default) => context.Users.SingleOrDefaultAsync(x => x.Email == email, ct); public Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default) => context.Users.AnyAsync(x => x.Email == email, ct); public Task<int> CountActiveAdministratorsAsync(CancellationToken ct = default) => context.Users.CountAsync(x => x.IsActive && x.Role == UserRole.Administrator, ct); public Task AddAsync(User user, CancellationToken ct = default) => context.Users.AddAsync(user, ct).AsTask(); public void Update(User user) => context.Users.Update(user); }
