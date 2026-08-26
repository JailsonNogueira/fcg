namespace FCG.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(Guid Id, string Name, string Email);
