using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FCG.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Obtém o identificador do usuário autenticado a partir do token.
    /// </summary>
    /// <param name="principal">Identidade da requisição atual.</param>
    /// <returns>Identificador do usuário autenticado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Lançada quando o token não carrega um identificador utilizável, o que só ocorre
    /// se o endpoint for exposto sem autenticação.
    /// </exception>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        // O JwtBearer mapeia "sub" para NameIdentifier por padrão, mas a leitura crua também é aceita.
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("O token não contém um identificador de usuário válido.");
    }
}
