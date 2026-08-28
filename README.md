# FCG — FIAP Cloud Games

API REST da **FIAP Cloud Games**, plataforma de venda de jogos digitais construída como **Tech Challenge — Fase 1** da pós-graduação FIAP.

---

## Sumário

1. [Objetivo](#objetivo)
2. [Checklist de requisitos](#checklist-de-requisitos)
3. [Arquitetura e DDD](#arquitetura-e-ddd)
4. [Pré-requisitos](#pré-requisitos)
5. [Como executar](#como-executar)
6. [Validação passo a passo](#validação-passo-a-passo)
7. [Endpoints](#endpoints)
8. [Regras de negócio](#regras-de-negócio)
9. [Tratamento de erros e logs](#tratamento-de-erros-e-logs)
10. [Testes](#testes)
11. [Modelagem DDD — Event Storming](#modelagem-ddd--event-storming)
12. [Pipeline CI/CD](#pipeline-cicd)
13. [Banco de dados](#banco-de-dados)

---

## Objetivo

Criar uma API REST em .NET para gerenciar **usuários e biblioteca de jogos adquiridos**, servindo como MVP da plataforma FIAP Cloud Games. O projeto aplica:

- **DDD** (Domain-Driven Design) na modelagem e organização do código;
- **Monolito em camadas** para desenvolvimento ágil do MVP;
- **Autenticação JWT** com dois níveis de acesso;
- **Testes unitários e BDD** para garantir qualidade;
- **Entity Framework Core** com migrations para persistência.

---

## Checklist de requisitos

Mapeamento direto entre cada requisito do enunciado e onde ele está implementado.

### Funcionalidades obrigatórias

| Requisito | Status | Onde verificar |
|---|---|---|
| Cadastro de usuários (nome, e-mail, senha) | ✅ | `POST /auth/register` — [AuthController.cs](src/FCG.Api/Controllers/AuthController.cs) |
| Validar formato de e-mail | ✅ | Value Object [Email.cs](src/FCG.Domain/Users/ValueObjects/Email.cs) — rejeita formatos inválidos |
| Validar senha segura (≥ 8 chars, letras, números, especiais) | ✅ | Value Object [Password.cs](src/FCG.Domain/Users/ValueObjects/Password.cs) |
| Autenticação via token JWT | ✅ | `POST /auth/login` — [JwtTokenGenerator.cs](src/FCG.Infrastructure/Security/JwtTokenGenerator.cs) |
| Nível Usuário — acesso à plataforma e biblioteca | ✅ | Role `Player` — policies `Library` e `Catalog` |
| Nível Administrador — cadastrar jogos, admin de usuários, promoções | ✅ | Role `Administrator` — policies `ManageCatalog`, `ManageUsers`, `ManagePromotions` |
| Monolito | ✅ | Solução única `FCG.sln` com camadas separadas por projeto |

### Requisitos técnicos

| Requisito | Status | Onde verificar |
|---|---|---|
| Entity Framework Core | ✅ | [FcgDbContext.cs](src/FCG.Infrastructure/Persistence/FcgDbContext.cs) — PostgreSQL |
| Migrations | ✅ | Pasta [Migrations/](src/FCG.Infrastructure/Persistence/Migrations/) — aplicadas automaticamente na inicialização |
| API .NET com Controllers MVC | ✅ | Pasta [Controllers/](src/FCG.Api/Controllers/) — 5 controllers |
| Middleware para tratamento de erros | ✅ | [ExceptionHandlingMiddleware.cs](src/FCG.Api/Middleware/ExceptionHandlingMiddleware.cs) — retorna `application/problem+json` |
| Logs estruturados | ✅ | Serilog — console + arquivo JSON diário (`logs/fcg-*.json`) via [CorrelationIdMiddleware.cs](src/FCG.Api/Middleware/CorrelationIdMiddleware.cs) |
| Swagger | ✅ | Configurado em [Program.cs](src/FCG.Api/Program.cs) — acessível em `/swagger` |
| Testes unitários | ✅ | Projeto [FCG.Tests.Unit](tests/FCG.Tests.Unit/) — xUnit |
| BDD em pelo menos um módulo | ✅ | Projeto [FCG.Tests.Bdd](tests/FCG.Tests.Bdd/) — Reqnroll + xUnit, 3 features, 20 cenários (26 casos executados) |
| DDD — Event Storming | ✅ | [Miro](https://miro.com/app/board/uXjVHxmXqYs=/) — fluxos de criação de jogos e usuários |
| DDD — organização de entidades e regras de negócio | ✅ | Camada [FCG.Domain](src/FCG.Domain/) — agregados, value objects, exceções de domínio |

---

## Arquitetura e DDD

### Estrutura de projetos

```
src/
  FCG.Domain            Agregados, Value Objects, exceções de domínio e contratos de repositório.
                         Sem dependência de frameworks ou infraestrutura.
  FCG.Application       Casos de uso (um handler por comando/consulta), DTOs e abstrações
                         (IUnitOfWork, IClock, IPasswordHasher, ITokenGenerator).
  FCG.Infrastructure    EF Core (PostgreSQL), repositórios, migrations, BCrypt e geração de JWT.
  FCG.Api               Controllers, políticas de autorização, middlewares e Swagger.

tests/
  FCG.Tests.Unit        Testes unitários de domínio e casos de uso (xUnit).
  FCG.Tests.Bdd         Cenários BDD em português (Reqnroll + xUnit).
  FCG.Tests.Shared      Repositórios em memória e fakes compartilhados entre os projetos de teste.
```

### Fluxo de dependência

```
FCG.Api  →  FCG.Application  →  FCG.Domain
                ↑                     ↑
          FCG.Infrastructure ─────────┘
```

A dependência aponta sempre para dentro (**Dependency Inversion**). `Infrastructure` implementa as interfaces declaradas no domínio.

### Agregados e Value Objects

| Agregado | Responsabilidade | Value Objects |
|---|---|---|
| `User` | Conta de jogador ou administrador | `Email`, `Password` |
| `Game` | Jogo do catálogo com nome, descrição e preço-base | — |
| `Promotion` | Desconto temporário sobre um jogo | — |
| `LibraryItem` | Registro de aquisição de um jogo por um jogador | — |

Cada agregado encapsula suas validações e regras — não há lógica de negócio fora da camada de domínio.

---

## Pré-requisitos

| Ferramenta | Versão |
|---|---|
| .NET SDK | 10.0+ (fixado em `global.json`) |
| Docker Desktop | Para o banco PostgreSQL local |
| Git | Para clonar o repositório |

> O enunciado da Fase 1 pede .NET 8; a turma foi autorizada a usar **.NET 10**, versão em que o projeto está
> fixado (`global.json`).

---

## Como executar

### 1. Clone o repositório

```bash
git clone https://github.com/JailsonNogueira/fcg.git
cd fcg
```

### 2. Suba o banco de dados

```bash
docker compose up -d
```

Aguarde o health check concluir (≈ 10 segundos):

```bash
docker compose ps
```

### 3. Configure as credenciais locais

Crie o arquivo `src/FCG.Api/appsettings.Development.json` — ele **não é versionado** (cada integrante cria o seu):

```json
{
  "Jwt": {
    "SecretKey": "fcg-dev-secret-key-com-no-minimo-32-caracteres"
  },
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=fcg;Username=fcg;Password=fcg_local_password"
  },
  "AdminSeed": {
    "Password": "Admin@123"
  }
}
```

O `appsettings.json` versionado já traz `Jwt:Issuer`/`Audience` e o e-mail do admin (`admin@fcg.com`), mas deixa
`Jwt:SecretKey`, `ConnectionStrings:Default` e `AdminSeed:Password` vazios — este arquivo os preenche. **Sem ele
a API não sobe** (`InvalidOperationException: Jwt:SecretKey não configurada`).

> **Em CI/CD ou produção**, sem o `appsettings.Development.json`, forneça as mesmas configurações por variáveis
> de ambiente (o `__` representa o aninhamento das chaves):
>
> ```bash
> ConnectionStrings__Default="Host=...;Port=5432;Database=fcg;Username=fcg;Password=..."
> Jwt__SecretKey="sua-chave-secreta-com-no-minimo-32-caracteres"
> AdminSeed__Password="senha-do-admin-inicial"
> ```

### 4. Rode a API

```bash
dotnet run --project src/FCG.Api
```

Na inicialização:
- As **migrations são aplicadas automaticamente**.
- Um **administrador padrão** é criado (seed) com base no `appsettings.Development.json`:

| Campo | Valor |
|---|---|
| E-mail | `admin@fcg.com` |
| Senha | `Admin@123` |

### 5. Acesse o Swagger

Abra no navegador: **http://localhost:5273/swagger**

> A porta pode variar conforme `Properties/launchSettings.json`.

---

## Validação passo a passo

Siga estes passos para verificar todos os requisitos do Tech Challenge.

### Passo 1 — Cadastro público de jogador

```bash
curl -s -X POST http://localhost:5273/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Alice","email":"alice@fcg.com","password":"Senha@123"}'
```

**Esperado:** `201 Created` com o `id` da conta criada (o header `Location` aponta para `api/users/{id}`).

**Validação de senha — rejeitar senha fraca:**

```bash
curl -s -X POST http://localhost:5273/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Bob","email":"bob@fcg.com","password":"1234"}'
```

**Esperado:** `400 Bad Request` — senha não atende à política (mínimo 8 caracteres, letras, números e especiais).

**Validação de e-mail — rejeitar formato inválido:**

```bash
curl -s -X POST http://localhost:5273/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Bob","email":"invalido","password":"Senha@123"}'
```

**Esperado:** `400 Bad Request` — formato de e-mail inválido.

**E-mail duplicado:**

```bash
curl -s -X POST http://localhost:5273/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Alice2","email":"alice@fcg.com","password":"Outra@456"}'
```

**Esperado:** `409 Conflict` — e-mail já cadastrado.

### Passo 2 — Autenticação JWT

**Login com administrador:**

```bash
curl -s -X POST http://localhost:5273/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fcg.com","password":"Admin@123"}'
```

**Esperado:** `200 OK` com `token` JWT, `userId`, `name`, `email`, `role: "Administrator"`.

> Copie o valor de `token` retornado. Nos passos seguintes, substitua `<TOKEN_ADMIN>` por ele.

**Login com jogador:**

```bash
curl -s -X POST http://localhost:5273/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@fcg.com","password":"Senha@123"}'
```

**Esperado:** `200 OK` com `role: "Player"`. Copie como `<TOKEN_PLAYER>`.

**Credenciais inválidas:**

```bash
curl -s -X POST http://localhost:5273/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fcg.com","password":"errada"}'
```

**Esperado:** `401 Unauthorized`.

### Passo 3 — Administrador cadastra um jogo

```bash
curl -s -X POST http://localhost:5273/api/games \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"name":"FIAP Adventure","description":"Jogo educativo de tecnologia","basePrice":99.90}'
```

**Esperado:** `201 Created` com o `id` do jogo. Copie como `<GAME_ID>`.

**Jogador não pode cadastrar jogos (autorização):**

```bash
curl -s -X POST http://localhost:5273/api/games \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_PLAYER>" \
  -d '{"name":"Outro Jogo","description":"Teste","basePrice":50}'
```

**Esperado:** `403 Forbidden` — apenas administradores podem cadastrar jogos.

### Passo 4 — Consultar catálogo

```bash
curl -s http://localhost:5273/api/games \
  -H "Authorization: Bearer <TOKEN_PLAYER>"
```

**Esperado:** `200 OK` com lista paginada contendo o jogo cadastrado, incluindo `basePrice`, `currentPrice` e `discountPercentage`.

### Passo 5 — Administrador cria promoção

```bash
curl -s -X POST http://localhost:5273/api/promotions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"gameId":"<GAME_ID>","discountPercentage":30,"startsAt":"2024-01-01T00:00:00Z","endsAt":"2099-12-31T23:59:59Z"}'
```

**Esperado:** `201 Created`. Ao consultar o catálogo novamente, o `currentPrice` será `69.93` (30% de desconto sobre 99.90).

### Passo 6 — Jogador adquire um jogo

```bash
curl -s -X POST http://localhost:5273/api/library \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_PLAYER>" \
  -d '{"gameId":"<GAME_ID>"}'
```

**Esperado:** `201 Created`. O preço pago é calculado no servidor (preço-base com promoção vigente aplicada).

**Adquirir o mesmo jogo novamente:**

```bash
curl -s -X POST http://localhost:5273/api/library \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_PLAYER>" \
  -d '{"gameId":"<GAME_ID>"}'
```

**Esperado:** `409 Conflict` — jogo já está na biblioteca.

### Passo 7 — Consultar biblioteca do jogador

```bash
curl -s http://localhost:5273/api/library \
  -H "Authorization: Bearer <TOKEN_PLAYER>"
```

**Esperado:** `200 OK` com a lista de jogos adquiridos, incluindo `pricePaid` e `acquiredAt`.

### Passo 8 — Administração de usuários

```bash
curl -s http://localhost:5273/api/users \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

**Esperado:** `200 OK` com lista paginada de todas as contas. Administradores podem visualizar, criar, editar, inativar e reativar contas.

### Passo 9 — Soft delete (exclusão lógica)

```bash
curl -s -X DELETE http://localhost:5273/api/games/<GAME_ID> \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

**Esperado:** `204 No Content`. O jogo é inativado, mas permanece na biblioteca de quem já comprou.

### Passo 10 — Executar os testes

```bash
dotnet test FCG.sln
```

**Esperado:** Todos os testes passam (unitários + BDD).

---

## Endpoints

Legenda de acesso: **Público** · **Autenticado** (qualquer perfil) · **Admin** (apenas `Administrator`) · **Jogador** (biblioteca pessoal).

### Autenticação — `/auth`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/auth/register` | Público | Cadastro de jogador. Valida formato de e-mail e senha segura. |
| POST | `/auth/login` | Público | Devolve o token JWT e os dados da conta. |

### Contas — `/api/users` (área administrativa)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/api/users` | Admin | Lista paginada. Filtros: `role`, `includeInactive`, `page`, `pageSize`. |
| GET | `/api/users/{id}` | Admin | Detalhe da conta. |
| POST | `/api/users` | Admin | Cria conta de `Player` ou `Administrator` (campo `role`). |
| PUT | `/api/users/{id}` | Admin | Atualiza nome e e-mail. |
| DELETE | `/api/users/{id}` | Admin | Inativa a conta (*soft delete*). Recusa inativar o último administrador ativo. |
| POST | `/api/users/{id}/activation` | Admin | Reativa uma conta inativada. |

### Catálogo — `/api/games`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/api/games` | Autenticado | Catálogo paginado com `basePrice`, `currentPrice` e `discountPercentage`. |
| GET | `/api/games/{id}` | Autenticado | Detalhe do jogo com preço vigente. |
| POST | `/api/games` | Admin | Cadastra jogo. Recusa nome duplicado. |
| PUT | `/api/games/{id}` | Admin | Atualiza nome, descrição e preço-base. |
| DELETE | `/api/games/{id}` | Admin | Retira do catálogo (*soft delete*). |
| POST | `/api/games/{id}/activation` | Admin | Devolve o jogo ao catálogo. |

### Promoções — `/api/promotions`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/api/promotions` | Admin | Lista paginada. Filtros: `gameId`, `includeDisabled`, `page`, `pageSize`. |
| GET | `/api/promotions/{id}` | Admin | Detalhe da promoção. |
| POST | `/api/promotions` | Admin | Cria promoção. Recusa sobreposição de vigência no mesmo jogo. |
| PUT | `/api/promotions/{id}` | Admin | Atualiza desconto e período. |
| DELETE | `/api/promotions/{id}` | Admin | Desabilita a promoção. |
| POST | `/api/promotions/{id}/activation` | Admin | Reabilita a promoção. |

### Biblioteca — `/api/library`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/api/library` | Jogador | Jogos adquiridos pela conta autenticada. |
| POST | `/api/library` | Jogador | Adquire um jogo. Corpo: `{ "gameId": "..." }`. |

> O jogador vem do token e **o preço é calculado no servidor** a partir do preço-base e da promoção vigente — nunca é aceito pelo corpo da requisição.

---

## Regras de negócio

| Regra | Detalhe |
|---|---|
| **Senha** | Mínimo 8 caracteres, com pelo menos uma letra, um número e um caractere especial. Sem espaços. Persistida apenas como hash BCrypt — nunca em texto aberto. |
| **E-mail** | Validado por formato, normalizado para minúsculas, único na plataforma. |
| **Nome duplicado de jogo** | Dois jogos não podem ter o mesmo nome (comparação case-insensitive). |
| **Promoções** | Desconto entre 0 (exclusivo) e 100; término posterior ao início; um jogo não pode ter duas promoções habilitadas com vigências sobrepostas. |
| **Aquisição** | O mesmo jogo não é adquirido duas vezes pela mesma conta; jogos inativos e contas inativas não adquirem. |
| **Exclusão lógica** | Contas, jogos e promoções são inativados (*soft delete*), preservando o histórico de compras. |
| **Último administrador** | A plataforma sempre mantém ao menos um administrador ativo — a inativação é recusada se for o último. |

---

## Tratamento de erros e logs

### Middleware de exceções

`ExceptionHandlingMiddleware` traduz exceções de domínio e aplicação em respostas padronizadas `application/problem+json`:

| Exceção | HTTP | Quando |
|---|---|---|
| `DomainException` | 400 | Violação de regra de negócio (senha fraca, e-mail inválido, preço negativo) |
| `UnauthorizedException` | 401 | Credenciais inválidas no login |
| `KeyNotFoundException` | 404 | Recurso não encontrado |
| `ConflictException` | 409 | E-mail duplicado, jogo já adquirido, promoção sobreposta |
| Qualquer outra | 500 | Erro inesperado |

### Logs estruturados

- **Serilog** grava no console e em arquivo JSON diário (`logs/fcg-*.json`, 30 dias de retenção).
- **`CorrelationIdMiddleware`** atribui um `correlationId` único a cada requisição, presente no corpo do erro e nos logs, facilitando rastreabilidade.

---

## Testes

### Como executar

```bash
dotnet test FCG.sln
```

### Testes unitários — `FCG.Tests.Unit`

Validam as regras dos agregados, value objects e todos os handlers da camada de aplicação, usando **repositórios em memória** (fakes) para isolamento total do banco.

Exemplos de cenários cobertos:
- Rejeitar e-mail em formato inválido
- Rejeitar senha que não atende à política
- Impedir cadastro com e-mail duplicado
- Impedir aquisição duplicada do mesmo jogo
- Calcular preço com promoção vigente
- Proteger inativação do último administrador

### Testes BDD — `FCG.Tests.Bdd`

Escritos em **português** com [Reqnroll](https://reqnroll.net/) (Gherkin + xUnit), exercitando os handlers da aplicação como caixa-preta. As features ficam em `tests/FCG.Tests.Bdd/Features/`:

| Feature | Cenários | Fluxo testado |
|---|---|---|
| `CadastroDeUsuario.feature` | 7 | Cadastro público, validação de senha e e-mail, e-mail duplicado, normalização, criação por admin |
| `Autenticacao.feature` | 5 | Login válido, senha incorreta, conta inexistente, conta inativa, e-mail malformado |
| `BibliotecaDeJogos.feature` | 8 | Aquisição sem/com promoção, promoção vencida, duplicidade, jogo inativo, conta inativa, isolamento entre jogadores |

**Total: 20 cenários** — `dotnet test` executa **26 casos** (os 2 cenários de cadastro são Esquemas de Cenário, que expandem em várias linhas de exemplos).

### Cobertura de código

```bash
dotnet test FCG.sln --collect:"XPlat Code Coverage" --results-directory TestResults
dotnet tool restore
dotnet tool run reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"CoverageReport" \
  -assemblyfilters:"+FCG.Domain" \
  -reporttypes:"Cobertura;Html"
```

O relatório HTML gerado em `CoverageReport/index.html` mostra a cobertura por classe. O pipeline de CI exige **mínimo de 70%** de cobertura de linhas para `FCG.Domain`.

---

## Modelagem DDD — Event Storming

O domínio foi mapeado por **Event Storming** no Miro, cobrindo os seguintes fluxos:

- **Jornada do Jogador:** cadastro → autenticação → consulta ao catálogo → aquisição de jogo → acesso à biblioteca
- **Jornada do Administrador:** autenticação → gestão de usuários → cadastro/edição de jogos → cadastro de promoções

📋 **Link do Miro:** https://miro.com/app/board/uXjVHxmXqYs=/

Os princípios de DDD aplicados no código:

| Princípio | Aplicação |
|---|---|
| **Aggregates** | `User`, `Game`, `Promotion`, `LibraryItem` — cada um com raiz e invariantes |
| **Value Objects** | `Email` (validação + normalização), `Password` (política de segurança) |
| **Domain Exceptions** | `DomainException` para violação de regras de negócio |
| **Repository Pattern** | Interfaces no domínio (`IUserRepository`, `IGameRepository`, etc.), implementações na infra |
| **Unit of Work** | `IUnitOfWork` na aplicação, implementado pelo `FcgDbContext` |
| **Dependency Inversion** | Domínio não conhece EF Core, JWT, BCrypt — tudo via interfaces |

---

## Pipeline CI/CD

O arquivo `.github/workflows/ci.yml` executa automaticamente em cada **push** e **pull request**:

1. **Restore** de dependências
2. **Build** da solução
3. **Testes** com coleta de cobertura
4. **Relatório de cobertura** publicado como artefato
5. **Validação de threshold** — falha se `FCG.Domain` tiver menos de 70% de cobertura

---

## Banco de dados

O `docker-compose.yml` sobe os seguintes serviços:

| Serviço | Porta | Uso |
|---|---|---|
| PostgreSQL | `localhost:5432` | Banco principal (EF Core) |
| MongoDB | `localhost:27017` | Disponível para uso futuro |

Ambos com volumes persistentes e health checks. As credenciais são exclusivas para desenvolvimento local.

```bash
docker compose ps          # verificar status
docker compose down -v     # derrubar e limpar volumes
```

Para gerar uma nova migration após alterar o modelo:

```bash
dotnet ef migrations add NomeDaMigration \
  --project src/FCG.Infrastructure \
  --startup-project src/FCG.Api
```
