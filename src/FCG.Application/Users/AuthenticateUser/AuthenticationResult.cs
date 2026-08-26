namespace FCG.Application.Users.AuthenticateUser;

public sealed record AuthenticationResult(
    string Token,
    Guid UserId,
    string Name,
    string Email,
    string Role);
