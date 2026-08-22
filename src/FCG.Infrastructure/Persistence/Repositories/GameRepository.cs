using FCG.Domain.Games;
using Microsoft.EntityFrameworkCore;
namespace FCG.Infrastructure.Persistence.Repositories;
public sealed class GameRepository(FcgDbContext context) : IGameRepository { public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default) => context.Games.SingleOrDefaultAsync(x => x.Id == id, ct); public Task<bool> ExistsByNormalizedNameAsync(string name, Guid? ignoredGameId = null, CancellationToken ct = default) => context.Games.AnyAsync(x => x.NormalizedName == name && (!ignoredGameId.HasValue || x.Id != ignoredGameId), ct); public Task AddAsync(Game game, CancellationToken ct = default) => context.Games.AddAsync(game, ct).AsTask(); public void Update(Game game) => context.Games.Update(game); }
