using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Users;
using FCG.Domain.Users.Enums;

namespace FCG.Application.Users.DeactivateUser;

public sealed class DeactivateUserHandler(IUserRepository users, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(DeactivateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("O usuário informado não foi encontrado.");

        if (!user.IsActive)
        {
            return;
        }

        // Sem administrador ativo ninguém conseguiria voltar a administrar a plataforma.
        if (user.Role == UserRole.Administrator
            && await users.CountActiveAdministratorsAsync(cancellationToken) <= 1)
        {
            throw new ConflictException("A plataforma deve manter ao menos um administrador ativo.");
        }

        user.Deactivate();

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
