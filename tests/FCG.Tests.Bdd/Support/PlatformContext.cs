using FCG.Application.Libraries.AddLibraryItem;
using FCG.Application.Libraries.GetPlayerLibrary;
using FCG.Application.Users.AuthenticateUser;
using FCG.Application.Users.RegisterUser;
using FCG.Domain.Games;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Bdd.Support;

/// <summary>
/// Estado compartilhado por um cenário: repositórios em memória, handlers reais da camada
/// de aplicação e o resultado da última ação executada.
/// </summary>
/// <remarks>
/// O Reqnroll cria uma instância por cenário, então cada cenário parte de uma plataforma vazia.
/// Os handlers exercitados aqui são exatamente os que a API usa em produção.
/// </remarks>
public sealed class PlatformContext
{
    /// <summary>Relógio fixo, para que promoções tenham vigência determinística.</summary>
    public static readonly DateTimeOffset Now = new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    public InMemoryUserRepository Users { get; } = new();

    public InMemoryGameRepository Games { get; } = new();

    public InMemoryPromotionRepository Promotions { get; } = new();

    public InMemoryLibraryItemRepository LibraryItems { get; } = new();

    public StubPasswordHasher PasswordHasher { get; } = new();

    public StubTokenGenerator TokenGenerator { get; } = new();

    /// <summary>Exceção lançada pela última ação, ou <see langword="null"/> se ela foi bem-sucedida.</summary>
    public Exception? CapturedError { get; private set; }

    /// <summary>Resultado da última autenticação bem-sucedida.</summary>
    public AuthenticationResult? Authentication { get; private set; }

    public RegisterUserHandler RegisterUser()
        => new(Users, PasswordHasher, new RecordingUnitOfWork());

    public AuthenticateUserHandler AuthenticateUser()
        => new(Users, PasswordHasher, TokenGenerator);

    public AddLibraryItemHandler AddLibraryItem()
        => new(Users, Games, Promotions, LibraryItems, new FixedClock(Now), new RecordingUnitOfWork());

    public GetPlayerLibraryHandler GetPlayerLibrary()
        => new(LibraryItems, Games);

    /// <summary>
    /// Executa uma ação guardando a exceção em vez de propagá-la, para que o passo
    /// "Então" possa afirmar sobre a falha esperada.
    /// </summary>
    public async Task ExecuteAsync(Func<Task> action)
    {
        CapturedError = null;

        try
        {
            await action();
        }
        catch (Exception exception)
        {
            CapturedError = exception;
        }
    }

    public async Task AuthenticateAsync(string email, string password)
    {
        Authentication = null;

        await ExecuteAsync(async () =>
            Authentication = await AuthenticateUser().HandleAsync(new AuthenticateUserCommand(email, password)));
    }

    public User UserByEmail(string email)
        => Users.Items.SingleOrDefault(user => user.Email.Equals(Email.Create(email)))
            ?? throw new InvalidOperationException($"Nenhuma conta cadastrada com o e-mail '{email}'.");

    public Game GameByName(string name)
        => Games.Items.SingleOrDefault(game => game.Name == name)
            ?? throw new InvalidOperationException($"Nenhum jogo cadastrado com o nome '{name}'.");
}
