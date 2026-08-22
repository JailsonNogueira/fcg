using DomainUser = FCG.Domain.Users.User;
using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Infrastructure.Persistence;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    IUserRepository userRepository,
    AppDbContext dbContext,
    BCryptPasswordHasher hasher,
    JwtTokenGenerator jwtGenerator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        Email email;
        Password password;

        try
        {
            email = Email.Create(request.Email);
            password = Password.Create(request.Password);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
            return Conflict(new { error = "E-mail já cadastrado." });

        var passwordHash = hasher.Hash(password);
        var user = DomainUser.CreatePlayer(request.Name?.Trim() ?? string.Empty, email, passwordHash);

        try
        {
            await userRepository.AddAsync(user, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return CreatedAtAction(nameof(Register), new { id = user.Id, email = user.Email.Value, role = user.Role.ToString() });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        Email email;
        try
        {
            email = Email.Create(request.Email);
        }
        catch (DomainException)
        {
            return Unauthorized(new { error = "Credenciais inválidas." });
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive || !hasher.Verify(request.Password ?? string.Empty, user.PasswordHash))
            return Unauthorized(new { error = "Credenciais inválidas." });

        var token = jwtGenerator.Generate(user);
        return Ok(new { token });
    }
}

public sealed record RegisterRequest(string? Name, string? Email, string? Password);
public sealed record LoginRequest(string? Email, string? Password);
