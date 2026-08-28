using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Infrastructure.Persistence;

/// <summary>
/// Fábrica usada apenas pelas ferramentas do EF Core em design time (por exemplo,
/// <c>dotnet ef migrations add</c>). Constrói o contexto sem inicializar o host da API,
/// evitando a exigência de configurações de runtime como <c>Jwt:SecretKey</c>.
/// </summary>
/// <remarks>
/// A connection string vem de <c>ConnectionStrings__Default</c> quando presente; caso contrário,
/// usa o valor de desenvolvimento local. Comandos que apenas geram artefatos (como
/// <c>migrations add</c>) não conectam ao banco; comandos como <c>database update</c> usam a
/// string informada.
/// </remarks>
public sealed class FcgDbContextFactory : IDesignTimeDbContextFactory<FcgDbContext>
{
    private const string DevelopmentConnectionString =
        "Host=localhost;Port=5432;Database=fcg;Username=fcg;Password=fcg_local_password";

    public FcgDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? DevelopmentConnectionString;

        var options = new DbContextOptionsBuilder<FcgDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FcgDbContext(options);
    }
}
