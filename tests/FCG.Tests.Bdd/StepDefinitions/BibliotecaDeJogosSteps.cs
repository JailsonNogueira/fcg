using System.Globalization;
using FCG.Application.Common;
using FCG.Application.Libraries.AddLibraryItem;
using FCG.Application.Libraries.GetPlayerLibrary;
using FCG.Domain.Games;
using FCG.Domain.Promotions;
using FCG.Tests.Bdd.Support;
using Reqnroll;
using Xunit;

namespace FCG.Tests.Bdd.StepDefinitions;

/// <summary>
/// Passos da funcionalidade de biblioteca de jogos adquiridos.
/// </summary>
[Binding]
public sealed class BibliotecaDeJogosSteps(PlatformContext platform)
{
    private static readonly CultureInfo Brazilian = CultureInfo.GetCultureInfo("pt-BR");

    [Given(@"^que existe o jogo ""(.*)"" custando ([\d.,]+)$")]
    public async Task DadoQueExisteOJogo(string nome, string preco)
    {
        await platform.Games.AddAsync(Game.Create(nome, $"Descrição de {nome}", ParsePrice(preco)));
    }

    [Given(@"^que o jogo ""(.*)"" está com uma promoção vigente de (\d+) por cento$")]
    public async Task DadoQueOJogoTemPromocaoVigente(string nome, decimal desconto)
    {
        var game = platform.GameByName(nome);

        await platform.Promotions.AddAsync(Promotion.Create(
            game.Id,
            desconto,
            PlatformContext.Now.AddDays(-1),
            PlatformContext.Now.AddDays(1)));
    }

    [Given(@"^que o jogo ""(.*)"" está com uma promoção encerrada de (\d+) por cento$")]
    public async Task DadoQueOJogoTemPromocaoEncerrada(string nome, decimal desconto)
    {
        var game = platform.GameByName(nome);

        await platform.Promotions.AddAsync(Promotion.Create(
            game.Id,
            desconto,
            PlatformContext.Now.AddDays(-10),
            PlatformContext.Now.AddDays(-5)));
    }

    [Given(@"^que o jogo ""(.*)"" foi retirado do catálogo$")]
    public void DadoQueOJogoFoiRetiradoDoCatalogo(string nome)
    {
        platform.GameByName(nome).Deactivate();
    }

    [Given(@"^que o jogador ""(.*)"" já possui o jogo ""(.*)""$")]
    public async Task DadoQueOJogadorJaPossuiOJogo(string email, string nome)
    {
        var command = new AddLibraryItemCommand(platform.UserByEmail(email).Id, platform.GameByName(nome).Id);

        await platform.AddLibraryItem().HandleAsync(command);
    }

    [When(@"^o jogador ""(.*)"" adquire o jogo ""(.*)""$")]
    public async Task QuandoOJogadorAdquireOJogo(string email, string nome)
    {
        var command = new AddLibraryItemCommand(platform.UserByEmail(email).Id, platform.GameByName(nome).Id);

        await platform.ExecuteAsync(() => platform.AddLibraryItem().HandleAsync(command));
    }

    [Then(@"^a aquisição deve ser concluída com sucesso$")]
    public void EntaoAAquisicaoDeveSerConcluida()
    {
        Assert.Null(platform.CapturedError);
    }

    [Then(@"^a aquisição deve ser recusada por conflito$")]
    public void EntaoAAquisicaoDeveSerRecusadaPorConflito()
    {
        Assert.IsType<ConflictException>(platform.CapturedError);
    }

    [Then(@"^a biblioteca de ""(.*)"" deve conter (\d+) jogos?$")]
    public async Task EntaoABibliotecaDeveConter(string email, int quantidade)
    {
        var library = await ReadLibraryAsync(email);

        Assert.Equal(quantidade, library.Count);
    }

    [Then(@"^o jogo ""(.*)"" deve constar na biblioteca de ""(.*)"" por ([\d.,]+)$")]
    public async Task EntaoOJogoDeveConstarPor(string nome, string email, string preco)
    {
        var library = await ReadLibraryAsync(email);
        var item = Assert.Single(library, entry => entry.GameName == nome);

        Assert.Equal(ParsePrice(preco), item.PricePaid);
    }

    private Task<IReadOnlyCollection<Application.Libraries.LibraryItemSummary>> ReadLibraryAsync(string email)
        => platform.GetPlayerLibrary().HandleAsync(new GetPlayerLibraryQuery(platform.UserByEmail(email).Id));

    private static decimal ParsePrice(string value)
        => decimal.Parse(value, NumberStyles.Number, Brazilian);
}
