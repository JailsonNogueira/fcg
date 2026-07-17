namespace FCG.Tests.Unit;

/// <summary>
/// Valida a configuração básica da infraestrutura de testes unitários.
/// </summary>
public sealed class TestInfrastructureTests
{
    /// <summary>
    /// Verifica que o xUnit está disponível para descoberta e execução dos testes.
    /// </summary>
    [Fact]
    public void XunitTestInfrastructureShouldBeAvailable()
    {
        Assert.IsType<FactAttribute>(new FactAttribute());
    }
}
