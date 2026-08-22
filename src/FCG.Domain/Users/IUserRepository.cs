using FCG.Domain.Users.ValueObjects;

namespace FCG.Domain.Users;

/// <summary>
/// Define as operações de persistência necessárias para o agregado de usuário.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtém um usuário pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Usuário encontrado ou <see langword="null"/>.</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um usuário pelo e-mail normalizado.
    /// </summary>
    /// <param name="email">E-mail procurado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Usuário encontrado ou <see langword="null"/>.</returns>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o e-mail já pertence a uma conta.
    /// </summary>
    /// <param name="email">E-mail procurado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns><see langword="true"/> quando o e-mail já estiver cadastrado.</returns>
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta os administradores ativos da plataforma.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Quantidade de administradores ativos.</returns>
    Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo usuário ao repositório.
    /// </summary>
    /// <param name="user">Usuário que será persistido.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca um usuário existente para atualização.
    /// </summary>
    /// <param name="user">Usuário alterado.</param>
    void Update(User user);
}
