using FCG.Application.Abstractions;

namespace FCG.Infrastructure.Security;

public sealed class BCryptPasswordHasherAdapter : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
}
