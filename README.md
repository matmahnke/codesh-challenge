# OrderGenerator & OrderAccumulator

Sistema de envio de ordens de compra/venda de ativos com controle de exposição financeira por ativo, limitada a R$ 1.000.000.

> This is a challenge by [Coodesh](https://coodesh.com/)

## Tecnologias

- **Backend (OrderAccumulator):** C# / .NET 10, ASP.NET Core Web API, Entity Framework Core, SQLite, FluentValidation
- **Testes:** xUnit, FluentAssertions, NSubstitute
- **Frontend (OrderGenerator):** React 19, JavaScript (JSX), Vite, Fetch API
- **Infra:** Docker, Docker Compose, Nginx

## Como rodar

### Com Docker Compose

Pré-requisito: Docker com Docker Compose.

```bash
docker compose up --build
```

Frontend: http://localhost:5174

API: http://localhost:5165

Para parar: `docker compose down` (adicione `-v` para apagar o banco de dados).

### Localmente

Pré-requisitos: .NET 10 SDK e Node.js 22+.

**Backend** (API em http://localhost:5164, as migrations são aplicadas ao iniciar):

```bash
cd OrderAccumulator
dotnet run --project src/Api --launch-profile http
```

**Frontend** (em outro terminal):

```bash
cd OrderGenerator
npm install
npm run dev
```

O endereço da API usado pelo frontend é definido em `OrderGenerator/.env` (`VITE_API_URL`). Ajuste para `http://localhost:5164` se o backend estiver rodando com o perfil `http`.

**Testes:**

```bash
cd OrderAccumulator
dotnet test
```

## API

`POST /api/orders`

```json
{ "ativo": "PETR4", "lado": "C", "quantidade": 584, "preco": 54.87 }
```

Resposta:

```json
{ "sucesso": true, "exposicao_atual": 32044.08, "msg_erro": null }
```

- `ativo`: `PETR4`, `VALE3` ou `VIIA4`
- `lado`: `C` (compra) ou `V` (venda)
- `quantidade`: inteiro entre 1 e 99.999
- `preco`: decimal positivo, múltiplo de 0,01 e menor que 1.000

Ordens que fariam a exposição do ativo ultrapassar R$ 1.000.000 (em valor absoluto) são rejeitadas e não registradas. Exemplos de requisição em `OrderAccumulator/src/Api/Api.http`.
