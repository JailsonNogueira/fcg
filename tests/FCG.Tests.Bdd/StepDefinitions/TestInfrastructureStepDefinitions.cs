using Reqnroll;
using Xunit;

namespace FCG.Tests.Bdd.StepDefinitions;

/// <summary>
/// Define os passos utilizados para validar a infraestrutura do projeto de testes BDD.
/// </summary>
[Binding]
public sealed class TestInfrastructureStepDefinitions
{
    private const string InfrastructureConfiguredKey = "InfrastructureConfigured";

    private readonly ScenarioContext scenarioContext;

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="TestInfrastructureStepDefinitions"/>.
    /// </summary>
    /// <param name="scenarioContext">Contexto compartilhado do cenário em execução.</param>
    public TestInfrastructureStepDefinitions(ScenarioContext scenarioContext)
    {
        this.scenarioContext = scenarioContext;
    }

    /// <summary>
    /// Registra que a infraestrutura de testes BDD está configurada.
    /// </summary>
    [Given("que a infraestrutura de testes BDD está configurada")]
    public void GivenTheBddTestInfrastructureIsConfigured()
    {
        scenarioContext[InfrastructureConfiguredKey] = true;
    }

    /// <summary>
    /// Verifica que o cenário foi descoberto e executado corretamente.
    /// </summary>
    [Then("o cenário deve ser executado com sucesso")]
    public void ThenTheScenarioShouldRunSuccessfully()
    {
        Assert.True(scenarioContext.Get<bool>(InfrastructureConfiguredKey));
    }
}
