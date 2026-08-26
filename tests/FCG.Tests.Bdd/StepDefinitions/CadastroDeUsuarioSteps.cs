using FCG.Application.Common;
using FCG.Application.Users.RegisterUser;
using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users.Enums;
using FCG.Tests.Bdd.Support;
using Reqnroll;
using Xunit;

namespace FCG.Tests.Bdd.StepDefinitions;

/// <summary>
/// Passos da funcionalidade de cadastro de usuários.
/// </summary>
[Binding]
public sealed class CadastroDeUsuarioSteps(PlatformContext platform)
{
    [When(@"^eu me cadastro com o nome ""(.*)"", o e-mail ""(.*)"" e a senha ""(.*)""$")]
    public async Task QuandoEuMeCadastro(string nome, string email, string senha)
    {
        await platform.ExecuteAsync(() =>
            platform.RegisterUser().HandleAsync(new RegisterUserCommand(nome, email, senha)));
    }

    [When(@"^um administrador cadastra a conta ""(.*)"" com o perfil ""(.*)""$")]
    public async Task QuandoUmAdministradorCadastraAConta(string email, string perfil)
    {
        var command = new RegisterUserCommand(
            "Conta Administrativa",
            email,
            "Senha@123",
            Enum.Parse<UserRole>(perfil));

        await platform.ExecuteAsync(() => platform.RegisterUser().HandleAsync(command));
    }

    [Then(@"^o cadastro deve ser concluído com sucesso$")]
    public void EntaoOCadastroDeveSerConcluido()
    {
        Assert.Null(platform.CapturedError);
    }

    [Then(@"^o cadastro deve ser recusado por dados inválidos$")]
    public void EntaoOCadastroDeveSerRecusadoPorDadosInvalidos()
    {
        Assert.IsType<DomainException>(platform.CapturedError);
    }

    [Then(@"^o cadastro deve ser recusado por conflito$")]
    public void EntaoOCadastroDeveSerRecusadoPorConflito()
    {
        Assert.IsType<ConflictException>(platform.CapturedError);
    }

    [Then(@"^nenhuma conta deve ter sido criada$")]
    public void EntaoNenhumaContaDeveTerSidoCriada()
    {
        Assert.Empty(platform.Users.Items);
    }

    [Then(@"^a plataforma deve ter (\d+) contas? cadastradas?$")]
    public void EntaoAPlataformaDeveTerContas(int quantidade)
    {
        Assert.Equal(quantidade, platform.Users.Items.Count);
    }

    [Then(@"^a conta ""(.*)"" deve ter o perfil ""(.*)""$")]
    public void EntaoAContaDeveTerOPerfil(string email, string perfil)
    {
        Assert.Equal(perfil, platform.UserByEmail(email).Role.ToString());
    }

    [Then(@"^a conta ""(.*)"" deve estar ativa$")]
    public void EntaoAContaDeveEstarAtiva(string email)
    {
        Assert.True(platform.UserByEmail(email).IsActive);
    }

    [Then(@"^a senha armazenada da conta ""(.*)"" não deve ser ""(.*)""$")]
    public void EntaoASenhaArmazenadaNaoDeveSerAOriginal(string email, string senha)
    {
        Assert.NotEqual(senha, platform.UserByEmail(email).PasswordHash);
    }
}
