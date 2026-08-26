using FCG.Application.Users.RegisterUser;
using FCG.Tests.Bdd.Support;
using Reqnroll;
using Xunit;

namespace FCG.Tests.Bdd.StepDefinitions;

/// <summary>
/// Passos de preparação compartilhados pelas funcionalidades de cadastro,
/// autenticação e biblioteca.
/// </summary>
[Binding]
public sealed class ContextoComumSteps(PlatformContext platform)
{
    [Given(@"^que a plataforma não possui usuários cadastrados$")]
    public void DadoQueAPlataformaEstaVazia()
    {
        Assert.Empty(platform.Users.Items);
    }

    [Given(@"^que existe um jogador ""(.*)"" com a senha ""(.*)""$")]
    public async Task DadoQueExisteUmJogador(string email, string senha)
    {
        await platform.RegisterUser().HandleAsync(new RegisterUserCommand("Jogador", email, senha));
    }

    [Given(@"^que a conta ""(.*)"" foi inativada$")]
    public void DadoQueAContaFoiInativada(string email)
    {
        platform.UserByEmail(email).Deactivate();
    }
}
