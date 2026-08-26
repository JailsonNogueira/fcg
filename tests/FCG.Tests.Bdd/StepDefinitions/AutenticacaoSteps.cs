using FCG.Application.Common;
using FCG.Application.Users.AuthenticateUser;
using FCG.Tests.Bdd.Support;
using Reqnroll;
using Xunit;

namespace FCG.Tests.Bdd.StepDefinitions;

/// <summary>
/// Passos da funcionalidade de autenticação.
/// </summary>
[Binding]
public sealed class AutenticacaoSteps(PlatformContext platform)
{
    /// <summary>Resultado da autenticação do cenário, com falha explícita se ela não ocorreu.</summary>
    private AuthenticationResult Authentication =>
        platform.Authentication
        ?? throw new InvalidOperationException("Nenhuma autenticação bem-sucedida foi executada no cenário.");

    [When(@"^eu autentico com o e-mail ""(.*)"" e a senha ""(.*)""$")]
    public async Task QuandoEuAutentico(string email, string senha)
    {
        await platform.AuthenticateAsync(email, senha);
    }

    [Then(@"^a autenticação deve ser concluída com sucesso$")]
    public void EntaoAAutenticacaoDeveSerConcluida()
    {
        Assert.Null(platform.CapturedError);
        Assert.NotNull(platform.Authentication);
    }

    [Then(@"^a autenticação deve ser recusada$")]
    public void EntaoAAutenticacaoDeveSerRecusada()
    {
        Assert.IsType<UnauthorizedException>(platform.CapturedError);
        Assert.Null(platform.Authentication);
    }

    [Then(@"^o token devolvido deve identificar a conta ""(.*)""$")]
    public void EntaoOTokenDeveIdentificarAConta(string email)
    {
        var user = platform.UserByEmail(email);

        Assert.Equal(email, Authentication.Email);
        Assert.Equal(user.Id, Authentication.UserId);
        Assert.Contains(user.Id.ToString(), Authentication.Token);
    }

    [Then(@"^o token devolvido deve carregar o perfil ""(.*)""$")]
    public void EntaoOTokenDeveCarregarOPerfil(string perfil)
    {
        Assert.Equal(perfil, Authentication.Role);
    }
}
