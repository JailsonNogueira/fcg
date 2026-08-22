# FCG — FIAP Cloud Games

Backend da plataforma FIAP Cloud Games, organizado inicialmente com foco no domínio e nas regras de negócio.

## Estrutura

- `src/FCG.Domain`: agregados, objetos de valor, exceções e contratos de repositório.
- `tests/FCG.Tests.Unit`: testes unitários do domínio com xUnit.
- `tests/FCG.Tests.Bdd`: infraestrutura de testes BDD com Reqnroll e xUnit.
- `.github/workflows/ci.yml`: validação de build, testes e cobertura em push e pull request.

## Requisitos

- .NET SDK 10.0.400 (definido em `global.json`)
- Docker Desktop, para os bancos de dados locais

## Executar localmente

```powershell
dotnet restore FCG.sln
dotnet build FCG.sln --no-restore
dotnet test FCG.sln
```

Para gerar dados de cobertura localmente:

```powershell
dotnet test FCG.sln --collect:"XPlat Code Coverage" --results-directory TestResults
dotnet tool restore
dotnet tool run reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -assemblyfilters:"+FCG.Domain" -reporttypes:"Cobertura;Html"
```

## Bancos de dados locais

```powershell
docker compose up -d
docker compose ps
```

O Compose disponibiliza PostgreSQL em `localhost:5432` e MongoDB em `localhost:27017`, com volumes persistentes e health checks. As credenciais atuais são exclusivamente para desenvolvimento local e estão declaradas no `docker-compose.yml`.

## Qualidade

O pipeline executa restore, build e testes em todos os pushes e pull requests. Também publica o relatório de cobertura e exige pelo menos 70% de cobertura de linhas para `FCG.Domain`.
