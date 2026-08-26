using FCG.Application.Abstractions;

namespace FCG.Infrastructure.Security;

public sealed class BCryptPasswordHasherAdapter : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash gravado em formato inesperado: trata como credencial inválida em vez de 500.
            return false;
        }
    }
}
