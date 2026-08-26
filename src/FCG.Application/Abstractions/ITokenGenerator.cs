using FCG.Domain.Users;

namespace FCG.Application.Abstractions;

public interface ITokenGenerator
{
    string Generate(User user);
}
