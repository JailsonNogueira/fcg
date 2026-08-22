# Como rodar o projeto FCG localmente

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução
- [.NET SDK 10.0.400](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) ou superior
- (Opcional) [DBeaver](https://dbeaver.io/) ou outro cliente SQL para inspecionar o banco

---

## 1. Clonar e restaurar dependências

```bash
git clone <url-do-repositório>
cd fcg
dotnet restore FCG.sln
```

---

## 2. Subir os bancos de dados

```bash
docker-compose up -d
```

Isso sobe:
| Serviço | Porta | Banco | Usuário | Senha |
|---------|-------|-------|---------|-------|
| PostgreSQL | 5432 | fcg | fcg | fcg_local_password |
| MongoDB | 27017 | fcg | fcg | fcg_local_password |

> **Nota:** MongoDB está provisionado para uso futuro (fases seguintes). Esta fase utiliza apenas PostgreSQL.

---

## 3. Configurar credenciais locais

Crie (ou edite) o arquivo `src/FCG.Api/appsettings.Development.json` com suas configurações locais:

```json
{
  "Jwt": {
    "SecretKey": "fcg-dev-secret-key-must-be-at-least-32-chars!"
  },
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=fcg;Username=fcg;Password=fcg_local_password"
  },
  "AdminSeed": {
    "Name": "Administrador FCG",
    "Email": "admin@fcg.com",
    "Password": "Admin@123"
  }
}
```

> Este arquivo **não está no controle de versão** (gitignored). Cada membro do grupo cria o seu localmente.

---

## 4. Aplicar as migrations

```bash
dotnet ef database update \
  --project src/FCG.Infrastructure \
  --startup-project src/FCG.Api
```

Isso cria as tabelas `users`, `games`, `library_items` e `promotions` no banco `fcg`.

---

## 5. Rodar a API

```bash
dotnet run --project src/FCG.Api
```

A API estará disponível em `http://localhost:5000` (ou a porta exibida no terminal).

---

## 6. Acessar o Swagger

Abra no navegador: `http://localhost:5000/swagger`

---

## 7. Testar os endpoints de autenticação

### Registrar um jogador

```bash
curl -s -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Alice","email":"alice@teste.com","password":"Senha@123"}' | jq .
```

Resposta esperada (`201 Created`):
```json
{
  "id": "...",
  "email": "alice@teste.com",
  "role": "Player"
}
```

### Fazer login e obter JWT

```bash
curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@teste.com","password":"Senha@123"}' | jq .
```

Resposta esperada (`200 OK`):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Login como administrador (criado automaticamente no startup)

```bash
curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fcg.com","password":"Admin@123"}' | jq .
```

---

## 8. Variáveis de ambiente para CI/CD

Para ambientes sem `appsettings.Development.json`, configure via variáveis de ambiente:

```bash
ConnectionStrings__Default="Host=...;Port=5432;Database=fcg;Username=fcg;Password=..."
Jwt__SecretKey="sua-chave-secreta-de-producao"
AdminSeed__Password="senha-do-admin-inicial"
```

---

## 9. Rodar os testes

```bash
dotnet test FCG.sln
```
