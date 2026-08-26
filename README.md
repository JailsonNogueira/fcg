# FCG — FIAP Cloud Games

API REST da **FIAP Cloud Games**, plataforma de venda de jogos digitais construída como Tech Challenge da Fase 1.

## Objetivo

Esta fase entrega o MVP de **cadastro de usuários e biblioteca de jogos adquiridos**, que servirá de base para as fases seguintes (matchmaking e gestão de servidores). O escopo cobre:

- cadastro público de jogadores, com validação de e-mail e política de senha segura;
- autenticação via **JWT** com dois níveis de acesso — `Player` e `Administrator`;
- gestão administrativa de contas, catálogo de jogos e promoções;
- aquisição de jogos pelo preço vigente e consulta da biblioteca pessoal.

O sistema é um **monolito** organizado em camadas segundo **DDD**, com o domínio isolado de infraestrutura e API.

## Arquitetura

```
src/
  FCG.Domain          agregados (User, Game, Promotion, LibraryItem), objetos de valor,
                      exceções de domínio e contratos de repositório — sem dependências externas
  FCG.Application     casos de uso (um handler por comando/consulta), DTOs e abstrações
                      (IUnitOfWork, IClock, IPasswordHasher, ITokenGenerator)
  FCG.Infrastructure  EF Core (PostgreSQL), repositórios, migrations, BCrypt e geração de JWT
  FCG.Api             controllers, políticas de autorização, middlewares e Swagger
tests/
  FCG.Tests.Unit      testes unitários de domínio e de casos de uso (xUnit)
  FCG.Tests.Bdd       cenários BDD em português (Reqnroll + xUnit)
  FCG.Tests.Shared    repositórios em memória compartilhados pelos dois projetos de teste
```

A dependência aponta sempre para dentro: `Api → Application → Domain`, com `Infrastructure` implementando as interfaces declaradas no domínio.

## Requisitos

- .NET SDK 10.0.400 (fixado em `global.json`)
- Docker Desktop, para o banco local

## Como executar

**1. Suba o banco:**

```bash
docker compose up -d
```

**2. Rode a API:**

```bash
dotnet run --project src/FCG.Api
```

As migrations são aplicadas automaticamente na inicialização e um administrador é semeado a partir de `AdminSeed` no `appsettings.Development.json`:

| Campo | Valor padrão (desenvolvimento) |
|---|---|
| E-mail | `admin@fcg.com` |
| Senha | `Admin@123` |

O Swagger fica em **`http://localhost:5273/swagger`** (ajuste a porta conforme o `launchSettings.json`).

**3. Autentique-se e use o token:**

```bash
curl -X POST http://localhost:5273/auth/login -H "Content-Type: application/json" -d '{"email":"admin@fcg.com","password":"Admin@123"}'
```

A resposta traz `token`. Envie-o em todas as chamadas protegidas no cabeçalho `Authorization: Bearer <token>`. No Swagger, use o botão **Authorize**.

## Endpoints

Legenda de acesso: **Público** · **Autenticado** (qualquer perfil) · **Admin** (apenas `Administrator`) · **Jogador** (biblioteca pessoal).

### Autenticação — `auth`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/auth/register` | Público | Cadastro de jogador. Valida formato de e-mail e senha segura. |
| POST | `/auth/login` | Público | Devolve o token JWT e os dados da conta. |

### Contas — `api/users` (área administrativa)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/api/users` | Admin | Lista paginada. Filtros: `role`, `includeInactive`, `page`, `pageSize`. |
| GET | `/api/users/{id}` | Admin | Detalhe da conta. |
| POST | `/api/users` | Admin | Cria conta de `Player` ou `Administrator` (campo `role`). |
| PUT | `/api/users/{id}` | Admin | Atualiza nome e e-mail. |
| DELETE | `/api/users/{id}` | Admin | Inativa a conta (*soft delete*). Recusa inativar o último administrador ativo. |
| POST | `/api/users/{id}/activation` | Admin | Reativa uma conta inativada. |

### Catálogo — `api/games`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/api/games` | Autenticado | Catálogo paginado com `basePrice`, `currentPrice` e `discountPercentage` já aplicados. Filtros: `search`, `includeInactive` (só admin), `page`, `pageSize`. |
| GET | `/api/games/{id}` | Autenticado | Detalhe do jogo com o preço vigente. |
| POST | `/api/games` | Admin | Cadastra um jogo. Recusa nome duplicado. |
| PUT | `/api/games/{id}` | Admin | Atualiza nome, descrição e preço-base. |
| DELETE | `/api/games/{id}` | Admin | Retira do catálogo (*soft delete*), preservando as bibliotecas de quem já comprou. |
| POST | `/api/games/{id}/activation` | Admin | Devolve o jogo ao catálogo. |

### Promoções — `api/promotions`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/api/promotions` | Admin | Lista paginada. Filtros: `gameId`, `includeDisabled`, `page`, `pageSize`. |
| GET | `/api/promotions/{id}` | Admin | Detalhe da promoção. |
| POST | `/api/promotions` | Admin | Cria promoção. Recusa sobreposição de vigência no mesmo jogo. |
| PUT | `/api/promotions/{id}` | Admin | Atualiza desconto e período. |
| DELETE | `/api/promotions/{id}` | Admin | Desabilita a promoção; o jogo volta ao preço-base. |
| POST | `/api/promotions/{id}/activation` | Admin | Reabilita a promoção. |

### Biblioteca — `api/library`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/api/library` | Jogador | Jogos adquiridos pela conta autenticada. |
| POST | `/api/library` | Jogador | Adquire um jogo. O corpo envia apenas `gameId`. |

> O jogador vem do token e **o preço é calculado no servidor** a partir do preço-base e da promoção vigente — nunca é aceito pelo corpo da requisição.

## Regras de negócio

- **Senha:** mínimo de 8 caracteres, contendo pelo menos uma letra, um número e um caractere especial; sem espaços. Persistida apenas como hash BCrypt.
- **E-mail:** validado por formato, normalizado para minúsculas e único na plataforma.
- **Catálogo:** dois jogos não podem compartilhar o mesmo nome (comparação normalizada, sem diferenciar maiúsculas).
- **Promoções:** desconto entre 0 (exclusivo) e 100; término posterior ao início; um jogo não pode ter duas promoções habilitadas com vigências sobrepostas.
- **Aquisição:** o mesmo jogo não é adquirido duas vezes pela mesma conta; jogos fora do catálogo e contas inativas não adquirem.
- **Exclusões são lógicas:** contas, jogos e promoções são inativados, preservando o histórico de compras.
- **Último administrador:** a plataforma sempre mantém ao menos um administrador ativo.

## Tratamento de erros e logs

`ExceptionHandlingMiddleware` traduz exceções para respostas `application/problem+json`:

| Exceção | HTTP |
|---|---|
| `DomainException` | 400 |
| `UnauthorizedException` | 401 |
| `ConflictException` | 409 |
| `KeyNotFoundException` | 404 |
| demais | 500 |

`CorrelationIdMiddleware` atribui um `correlationId` a cada requisição, devolvido no corpo do erro e presente nos logs. O **Serilog** grava no console e em arquivo JSON diário (`logs/fcg-*.json`, 30 dias de retenção).

## Testes

```bash
dotnet test FCG.sln
```

- **Unitários** (`FCG.Tests.Unit`): regras dos agregados e objetos de valor, e todos os casos de uso da camada de aplicação, com repositórios em memória.
- **BDD** (`FCG.Tests.Bdd`): cenários escritos em português com Reqnroll, cobrindo cadastro de usuários, autenticação e biblioteca de jogos. As features estão em `tests/FCG.Tests.Bdd/Features` e exercitam os mesmos handlers usados pela API.

Cobertura local:

```bash
dotnet test FCG.sln --collect:"XPlat Code Coverage" --results-directory TestResults
dotnet tool restore
dotnet tool run reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -assemblyfilters:"+FCG.Domain" -reporttypes:"Cobertura;Html"
```

O pipeline em `.github/workflows/ci.yml` executa restore, build e testes em cada push e pull request, publica o relatório de cobertura e exige no mínimo 70% de cobertura de linhas para `FCG.Domain`.

## Modelagem (DDD)

O domínio foi mapeado por **Event Storming**, cobrindo as jornadas do jogador (cadastro, autenticação, consulta ao catálogo e aquisição, acesso à biblioteca) e do administrador (cadastro e autenticação, gestão de administradores, cadastro e edição de jogos, cadastro de promoções).

> Documentação da modelagem: https://miro.com/app/board/uXjVH4alQ5U=/

## Banco de dados

O `docker-compose.yml` sobe PostgreSQL em `localhost:5432` e MongoDB em `localhost:27017`, com volumes persistentes e health checks. As credenciais declaradas ali são exclusivas para desenvolvimento local.

```bash
docker compose ps
```

Para gerar uma nova migration após alterar o modelo:

```bash
dotnet ef migrations add NomeDaMigration --project src/FCG.Infrastructure --startup-project src/FCG.Api
```
