using FCG.Domain.Users.ValueObjects;

namespace FCG.Infrastructure.Security;

public sealed class BCryptPasswordHasher
{
    public string Hash(Password password)
        => BCrypt.Net.BCrypt.HashPassword(password.Value);

    public bool Verify(string plainText, string hash)
        => BCrypt.Net.BCrypt.Verify(plainText, hash);
}
