using FCG.Application.Abstractions;
namespace FCG.Infrastructure.Persistence;
public sealed class UnitOfWork(FcgDbContext context) : IUnitOfWork { public Task SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken); }
