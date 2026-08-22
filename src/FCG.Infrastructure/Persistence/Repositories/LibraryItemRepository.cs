using FCG.Domain.Libraries;
using Microsoft.EntityFrameworkCore;
namespace FCG.Infrastructure.Persistence.Repositories;
public sealed class LibraryItemRepository(FcgDbContext context) : ILibraryItemRepository { public Task<bool> ExistsAsync(Guid playerId, Guid gameId, CancellationToken ct = default) => context.LibraryItems.AnyAsync(x => x.PlayerId == playerId && x.GameId == gameId, ct); public async Task<IReadOnlyCollection<LibraryItem>> GetByPlayerIdAsync(Guid playerId, CancellationToken ct = default) => await context.LibraryItems.Where(x => x.PlayerId == playerId).ToListAsync(ct); public Task AddAsync(LibraryItem item, CancellationToken ct = default) => context.LibraryItems.AddAsync(item, ct).AsTask(); }
