namespace FCG.Domain.Users.Enums;

/// <summary>
/// Define os perfis de acesso disponíveis na plataforma.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Usuário que acessa o catálogo e a própria biblioteca de jogos.
    /// </summary>
    Player = 1,

    /// <summary>
    /// Usuário que acessa exclusivamente as funcionalidades administrativas.
    /// </summary>
    Administrator = 2
}
