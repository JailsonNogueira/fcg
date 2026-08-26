namespace FCG.Application.Users;

/// <summary>
/// Projeção de leitura de uma conta da plataforma.
/// </summary>
public sealed record UserSummary(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool IsActive);
