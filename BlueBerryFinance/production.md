# BlueberryFinance — Production Specification

> Personal Financial Control Assistant
> Stack: .NET 10 API · .NET 9 Blazor Web · Docker (WSL2) · PostgreSQL · RabbitMQ · LibreChat · LiteLLM

---

## ⚠️ Security Actions Required Before Any Work

1. Move all secrets to **User Secrets** (`dotnet user-secrets`) for local dev and to **environment variables / Docker secrets** for containers. `appsettings.Development.json` must never contain real keys.
2. Add `appsettings.Development.json` to `.gitignore` as a safety net (keep only the sanitized version in source control with empty/example values).

---
©
## Table of Contents

1. [Project Layout](#1-project-layout)
2. [Domain Model](#2-domain-model)
3. [API Architecture](#3-api-architecture)
4. [Web / UI Architecture](#4-web--ui-architecture)
5. [Agent System](#5-agent-system)
6. [Jobs & RabbitMQ](#6-jobs--rabbitmq)
7. [Authentication & Authorization](#7-authentication--authorization)
8. [Logging & Audit](#8-logging--audit)
9. [Integrations](#9-integrations)
10. [Docker Compose](#10-docker-compose)
11. [Testing Strategy](#11-testing-strategy)
12. [Implementation Phases](#12-implementation-phases)

---

## 1. Project Layout

```
BlueBerryFinanceDotNet/
└── BlueBerryFinance/
    ├── BlueBerryFinance.slnx
    │
    ├── BlueBerryFinance.API/               ← .NET 10 Web API
    │   ├── Application/
    │   │   ├── Handlers/                   ← one handler per use case (IRequestHandler<TReq,TRes>) (with fluent validator)
    │   │   ├── Requests/
    │   │   ├── Responses/
    │   │   └── ViewModels/                 ← outbound view shapes (never domain entities)
    │   ├── Controllers/                    ← thin, SRP, try/catch at entry
    │   ├── Data/
    │   │   ├── Context/                    ← AppDbContext (EF Core, no repositories)
    │   │   └── Entities/                   ← domain entities + enums
    │   └── Infrastructure/
    │       ├── Agents/                     ← AI agents + tools
    │       ├── Factories/
    │       ├── Helpers/
    │       ├── Jobs/                       ← Hangfire/hosted service jobs
    │       ├── Messaging/                  ← RabbitMQ publishers & consumers
    │       ├── Models/
    │       └── Services/
    │
    ├── BlueberryFinance.Web/               ← .NET 9 Blazor Server (MudBlazor)
    │   ├── Clients/                        ← one typed HttpClient per context
    │   ├── Components/
    │   │   ├── Layout/
    │   │   ├── Pages/
    │   │   └── Shared/                     ← generic reusable components
    │   └── wwwroot/
    │       └── css/
    │           ├── _variables.css          ← single color/theme source of truth
    │           └── pages/                  ← one CSS file per page
    │
    ├── BlueBerryFinance.Common/            ← shared between API and Web
    │   ├── DomainObjects/                  ← Entity, EntityBase
    │   └── ViewModels/                     ← shared VM contracts (no domain entities here)
    │
    └── BlueBerryFinance.Tests/             ← xUnit test project
        ├── API/
        │   ├── Handlers/
        │   ├── Controllers/
        │   └── Agents/
        ├── Web/
        └── Data/                           ← mocks, test factories (DI-instantiated)
```

---

## 2. Domain Model

### 2.1 Entities (API only — never exposed to Web directly)

```
User
  Id, Email, Name, ProfileId, Active, InsertionDate, LastModification

BankAccount
  Id, UserId, Name, Bank (enum), Country (BRL|EUR), CurrencyId, Balance, Active

Currency
  Id, Code (BRL|EUR), Symbol, Name

Category
  Id, Name, Icon, Color, Type (Income|Expense)

Store
  Id, Name, CategoryId, Active

FixedExpense
  Id, UserId, Name, Description, Amount, CurrencyId, DayOfMonth, IsRecurring,
  StoreId, Active

Transaction
  Id, UserId, BankAccountId, StoreId, CategoryId, CurrencyId, FixedExpenseId (nullable),
  OriginTypeId, TransactionType (Income|Expense), Amount, Description,
  TransactionDate, Source (Manual|Bank|WhatsApp|Agent), ImageUrl (nullable),
  CorrelationId, InsertionDate, LastModification, IsDeleted, DeletionDate

FiscalNote
  Id, UserId, TransactionId (nullable), ImageUrl, RawText, ExtractedData (json),
  Status (Pending|Processed|Failed), InsertionDate

AuditLog
  Id, CorrelationId, UserId, Controller, Action, Payload (json), Response (json),
  StatusCode, DurationMs, InsertionDate
  (stored in a separate database / schema — see §8)

AgentApproval
  Id, UserId, AgentName, Tool, Payload (json), Status (Pending|Approved|Rejected),
  InsertionDate, ResolvedAt
```

### 2.2 Enums (remain in API + Common where shared)

```csharp
TransactionType  { Income, Expense }
TransactionSource{ Manual, BankSync, WhatsApp, Agent }
OriginType       { Person, Company, Store }
Country          { Brazil, Portugal }
CurrencyCode     { BRL, EUR }
ApprovalStatus   { Pending, Approved, Rejected }
```

### 2.3 Modeling Notes

- All entities inherit `EntityBase` from `BlueBerryFinance.Common`.
- `UserId` on every user-scoped entity ensures row-level data isolation.
- `AuditLog` lives in a dedicated DB (or schema) with a composite index on `(UserId, InsertionDate)` and a partial index on `CorrelationId` — never co-mingled with transactional data to avoid auditory queries slowing the main DB.
- Keep `Category` as a proper table (not enum) so users can add custom categories without code changes.
- `FiscalNote` stores the raw image URL (blob storage) and the agent-extracted structured data separately.

---

## 3. API Architecture

### 3.1 Controller Rules

- Inherits `BaseController : ControllerBase` with `[ApiController]` and `[Route("api/v1.0/")]`.
- **Single responsibility**: receive request → call handler → return typed response with explicit status codes.
- **try/catch at controller level**: catch `ValidationException` → 400, `UnauthorizedException` → 401, `NotFoundException` → 404, `Exception` → 500. Log full stack trace before returning.
- Never inject `DbContext` or agent directly into controllers — always through a handler.
- Action endpoints use verbs: `POST /transaction/register`, `POST /transaction/categorize`, `POST /report/monthly`.
- CRUD endpoints are separate from action endpoints:

```
GET    /api/v1.0/transaction          ← list (paged, filtered)
GET    /api/v1.0/transaction/{id}     ← detail
POST   /api/v1.0/transaction          ← create (action: register)
PUT    /api/v1.0/transaction/{id}     ← update
DELETE /api/v1.0/transaction/{id}     ← soft delete

POST   /api/v1.0/transaction/analyze  ← action: AI analysis
POST   /api/v1.0/report/monthly       ← action: trigger monthly report job
POST   /api/v1.0/agent/approval/{id}/approve
POST   /api/v1.0/agent/approval/{id}/reject

GET    /api/v1.0/chat/stream          ← SSE stream for LibreChat / Bot Framework
POST   /api/v1.0/whatsapp/webhook     ← WhatsApp incoming message
```

### 3.2 Handler Pattern

```csharp
// Application/Handlers/Interfaces/IRequestHandler.cs
public interface IRequestHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}

// One handler class per use-case, named after the use-case:
// RegisterTransactionHandler, AnalyzeTransactionsHandler, GenerateMonthlyReportHandler ...
```

Handlers are the **only** place business logic lives. They may call:
- `AppDbContext` directly (EF Core, custom LINQ per case — no generic repo).
- Agent interfaces.
- RabbitMQ publisher.
- External service interfaces (bank sync, WhatsApp API).

### 3.3 Data Access (EF Core, no repositories)

```csharp
// Data/Context/AppDbContext.cs
public class AppDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    // ...
}
```

- Handlers inject `AppDbContext` directly.
- Use `AsNoTracking()` for read queries; track only when writing.
- Write custom LINQ per handler — do not abstract into generic repositories.
- EF migrations live in `Data/Migrations/`.
- Wrap writes in `try/catch`: catch `DbUpdateException` and rethrow as domain exception with meaningful message.

### 3.4 Streaming Chat Endpoint (LibreChat / Bot Framework / Teams / Discord)

```csharp
// Controllers/ChatController.cs
[HttpGet("stream")]
public async Task Stream([FromQuery] string message, [FromQuery] string conversationId,
    CancellationToken ct)
{
    Response.Headers["Content-Type"] = "text/event-stream";
    Response.Headers["Cache-Control"] = "no-cache";
    await foreach (var chunk in _chatHandler.StreamAsync(message, conversationId, ct))
    {
        await Response.WriteAsync($"data: {chunk}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
```

- The handler calls the agent which wraps LiteLLM (via OpenAI-compatible client pointing at `http://litellm:4000`).
- Bot Framework adapter sits in `Infrastructure/Messaging/BotFramework/` — translates Teams/Discord messages to the same stream handler.
- LibreChat uses the same `/chat/stream` endpoint authenticated with a service JWT.

### 3.5 Configuration (appsettings)

`appsettings.json` — empty/example values only, committed to source control:

```json
{
  "AllowedHosts": "*",
  "Jwt": { "Secret": "", "Issuer": "", "Audience": "", "ExpiryDays": 1 },
  "OpenAI": { "ApiKey": "", "Provider": "LiteLLM", "BaseUrl": "" },
  "RabbitMQ": { "Host": "", "User": "", "Password": "", "VirtualHost": "/" },
  "ConnectionStrings": { "Default": "", "Audit": "" },
  "WhatsApp": { "Token": "", "VerifyToken": "" },
  "Logging": { "LogLevel": { "Default": "Information" } }
}
```

`appsettings.Development.json` — values filled at runtime via **User Secrets** or `.env`. Never commit real keys.

---

## 4. Web / UI Architecture

### 4.1 Technology

- **Blazor Server** (.NET 9) with **MudBlazor** component library.
- **ASP.NET Core Identity** for authentication (cookie-based for the UI).
- Admin email: `mateus@hotmail.com` (seeded at startup, profile: Admin).
- Authorization profiles: `Admin`, `User` (defined as policy constants).

### 4.2 HTTP Client Architecture

One typed `HttpClient` per domain context, registered in DI:

```csharp
// Clients/TransactionClient.cs
public class TransactionClient(HttpClient http)
{
    public Task<PagedResult<TransactionViewModel>> GetAsync(TransactionFilter filter, CancellationToken ct) ...
    public Task<TransactionViewModel> CreateAsync(RegisterTransactionRequest req, CancellationToken ct) ...
}
```

All clients:
- Inject `IHttpContextAccessor` to forward the JWT and `X-Correlation-Id` header on every outbound request.
- Throw typed exceptions for non-2xx responses that pages can catch and display.
- Never contain business logic.

### 4.3 Component Rules

- **One CSS file per page** in `wwwroot/css/pages/`.
- **`wwwroot/css/_variables.css`** is the single source of truth for colors, spacing, font.
- Generic reusable components live in `Components/Shared/` — parameterize everything, no duplication.
- Pages only contain rendering logic (state binding, UI events). No HTTP calls inline — always through a client.
- Never reference domain entities — only ViewModels from `BlueBerryFinance.Common/ViewModels/`.

### 4.4 Correlation ID & JWT Forwarding

```csharp
// Web DelegatingHandler added to every typed client
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
{
    var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"]
        .FirstOrDefault() ?? Guid.NewGuid().ToString();
    request.Headers.Add("X-Correlation-Id", correlationId);

    var jwt = await _tokenService.GetTokenAsync();
    if (jwt is not null)
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

    return await base.SendAsync(request, ct);
}
```

### 4.5 Design Language

- Corporate/modern: dark navy sidebar, white content area, accent color (configurable via `--color-accent` CSS variable).
- MudBlazor theme customized in a single `ThemeProvider` component injected in `MainLayout`.
- Dashboard, Transactions, Reports, Accounts, Settings, Chat — top-level nav items.
- Responsive — mobile-first grid.

---

## 5. Agent System

### 5.1 Architecture

```
Infrastructure/Agents/
  AgentBase<T>                ← abstract, handles structured JSON response + chat options
  IAgentBase<T>
  Finance/
    FinancialAssistantAgent   ← conversational, has tools
    ReportAgent               ← report generation
    FiscalNoteAgent           ← image reading (fiscal notes)
  Tools/
    GetTransactionsTool
    GetBankAccountsTool
    WriteTransactionTool      ← requires user approval
    WriteFiscalNoteTool       ← requires user approval
```

### 5.2 Tool Rules

- **Read tools** execute immediately.
- **Write tools** create an `AgentApproval` record (status=Pending), pause, and return a `ApprovalRequired` response to the user/UI.
- The user approves or rejects via `POST /api/v1.0/agent/approval/{id}/approve|reject`.
- After approval the job resumes and performs the write.

### 5.3 Image Input

Agents that accept images (`FiscalNoteAgent`) accept:
- Base64 image from LibreChat multipart upload.
- Image URL from WhatsApp webhook.
- File upload from Web UI.

All images are stored in blob storage (MinIO container) before agent processing. The agent reads from the stored URL.

### 5.4 LiteLLM Integration

All agents call **LiteLLM** as an OpenAI-compatible proxy (`http://litellm:4000/v1`).
LiteLLM is configured to route to `gpt-4.1-mini` (the only model accessible to users).
API key for LiteLLM is a virtual key issued per consumer (LibreChat gets its own key, the API gets its own key).

```csharp
// AIAgentFactory — target LiteLLM, not OpenAI directly
var client = new OpenAIClient(
    new ApiKeyCredential(options.LiteLLMApiKey),
    new OpenAIClientOptions { Endpoint = new Uri(options.LiteLLMBaseUrl) }
);
```

---

## 6. Jobs & RabbitMQ

### 6.1 Queues

| Queue | Purpose |
|---|---|
| `finance.report.monthly` | Trigger monthly financial report generation |
| `finance.bank.sync` | Trigger bank statement import |
| `finance.fiscal.note` | Process incoming fiscal note image |
| `finance.notification` | Send user notifications (email, WhatsApp) |

### 6.2 Recurring Jobs (Hangfire or IHostedService + cron)

| Job | Schedule | Description |
|---|---|---|
| `MonthlyReportJob` | 1st of month 08:00 | Generate PDF report, store, notify user |
| `BankSyncJob` | Daily 06:00 | Pull transactions from connected banks |
| `PendingApprovalReminderJob` | Every 4h | Notify user of pending agent approvals |

### 6.3 Pattern

```csharp
// Infrastructure/Messaging/Publishers/IMessagePublisher.cs
public interface IMessagePublisher
{
    Task PublishAsync<T>(string queue, T message, CancellationToken ct = default);
}

// Infrastructure/Messaging/Consumers/FiscalNoteConsumer.cs
// Listens on finance.fiscal.note, calls FiscalNoteAgent, creates Transaction
```

- RabbitMQ connection string read from environment.
- Use durable queues with manual ack — never auto-ack.
- Dead-letter queue for failed messages.

---

## 7. Authentication & Authorization

### 7.1 API (JWT)

- JWT issued by the API on `POST /api/v1.0/auth/login`.
- Valid for **1 day** (`ExpiryDays: 1` in config).
- Claims: `userId`, `email`, `profile` (Admin/User).
- Middleware: `UseAuthentication()` + `UseAuthorization()` in `Program.cs`.
- All endpoints require `[Authorize]` except `/auth/login` and `/whatsapp/webhook`.
- LibreChat authenticates using a **service account JWT** (long-lived, never expire, stored in LibreChat env).

### 7.2 Web (ASP.NET Identity + Cookie)

- ASP.NET Core Identity backed by the same PostgreSQL DB.
- Cookie auth for Blazor Server (not JWT — cookies are more suitable for server-rendered Blazor).
- The Web project exchanges the Identity cookie for a JWT when calling the API (via a token exchange service).
- Authorization policies:

```csharp
services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireClaim("profile", "Admin"));
    opt.AddPolicy("UserOrAdmin", p => p.RequireClaim("profile", "Admin", "User"));
});
```

- Default admin seeded: `mateus@hotmail.com` / password from `.env` `DEFAULT_ADMIN_PASSWORD`.

### 7.3 LibreChat User Control

- LibreChat users are mirrored to the `User` table via a webhook on LibreChat user creation.
- Tool access is controlled per user via a `UserToolAccess` table.
- The API enforces tool access on every agent tool call.

---

## 8. Logging & Audit

### 8.1 Audit Log DB

- Separate PostgreSQL DB (or schema): `blueberry_audit`.
- `AuditLog` table with columns: `Id`, `CorrelationId`, `UserId`, `Controller`, `Action`, `Method`, `Payload` (jsonb), `Response` (jsonb), `StatusCode`, `DurationMs`, `InsertionDate`.
- Indexes:
  - `(UserId, InsertionDate DESC)` — user history queries.
  - `(CorrelationId)` — trace a single request end-to-end.
  - Partial index `WHERE StatusCode >= 400` — error auditing.
- Partition by month if volume grows.

### 8.2 Middleware

```csharp
// Infrastructure/Middleware/AuditMiddleware.cs
// Runs after auth, before routing.
// 1. Read or generate X-Correlation-Id header.
// 2. Buffer request body (stream copy).
// 3. Wrap response body stream.
// 4. After handler completes: write AuditLog record.
// 5. Forward X-Correlation-Id on response.
```

- Payload and response are stored as raw JSON (truncated at 32 KB to avoid BLOB bloat).
- Sensitive fields (passwords, tokens) are redacted before storage via a configurable filter list.

### 8.3 Structured Logging

- Use **Serilog** with enrichers: `CorrelationId`, `UserId`, `MachineName`.
- Sink to **Seq** container (see §10) for development; production can add ElasticSearch.
- Log levels: Debug (dev only), Information (prod default), Warning, Error.

---

## 9. Integrations

### 9.1 Bank Integration (Brazil & Portugal)

**Strategy**: Use free, open, secure APIs where available.

| Country | Option | Notes |
|---|---|---|
| Brazil | **Pluggy** (free tier) or **Belvo** | Open Finance / Open Banking aggregators. OAuth2 per bank. |
| Portugal | **Nordigen / GoCardless** (free tier) | PSD2 compliant Open Banking. |

- Both require user consent (OAuth2 redirect flow).
- The API stores bank-specific OAuth tokens encrypted (AES-256, key in env).
- `BankSyncJob` calls the aggregator API, maps transactions, and publishes to `finance.bank.sync` queue.
- **Security note**: discuss with user before implementing OAuth flows — ensure redirect URIs are HTTPS only, tokens stored encrypted, and refresh rotation is implemented.

### 9.2 WhatsApp (Free Tier)

- Use **WhatsApp Business Platform (Meta Cloud API)** — free tier supports incoming messages and image messages.
- Webhook: `POST /api/v1.0/whatsapp/webhook`.
- Verify token checked against env `WHATSAPP_VERIFY_TOKEN`.
- Incoming image message → download media → store in MinIO → publish to `finance.fiscal.note` queue → `FiscalNoteAgent` processes.
- Reply to user with confirmation message via the WhatsApp send message API.
- **Security**: Validate `X-Hub-Signature-256` header on every webhook call.

### 9.3 LibreChat ↔ API

- LibreChat configured with a custom OpenAI-compatible endpoint pointing to `/api/v1.0/chat/stream`.
- LibreChat sends a service JWT (configured in LibreChat env `OPENAI_API_KEY`).
- The API validates the JWT, identifies the LibreChat service account, and routes to the correct agent.
- Users authenticated in LibreChat are matched to BlueberryFinance users by email.

---

## 10. Docker Compose

### 10.1 File Structure

```
docker/
├── .env                    ← all variables, never commit real secrets
├── docker-compose.yml      ← all services
├── litellm/
│   └── config.yaml         ← model routing config
└── librechat/
    └── librechat.yaml      ← LibreChat config
```

### 10.2 `.env` Template

```env
# === Identity ===
DEFAULT_ADMIN_EMAIL=mateus@hotmail.com
DEFAULT_ADMIN_PASSWORD=ChangeMe123!
DEFAULT_DOMAIN=localhost

# === PostgreSQL (main) ===
POSTGRES_HOST=local-dev-postgres
POSTGRES_PORT=5432
POSTGRES_DB=blueberry_finance
POSTGRES_USER=blueberry
POSTGRES_PASSWORD=ChangeMe123!
POSTGRES_AUDIT_DB=blueberry_audit

# === RabbitMQ ===
RABBITMQ_HOST=local-dev-rabbitmq
RABBITMQ_USER=blueberry
RABBITMQ_PASSWORD=ChangeMe123!
RABBITMQ_VHOST=/

# === MinIO (blob storage) ===
MINIO_ROOT_USER=blueberry
MINIO_ROOT_PASSWORD=ChangeMe123!
MINIO_BUCKET=blueberry-files

# === LiteLLM ===
LITELLM_MASTER_KEY=sk-litellm-master-key
LITELLM_LIBRECHAT_KEY=sk-litellm-librechat-key
LITELLM_API_KEY=sk-litellm-api-key
OPENAI_API_KEY=                             # real key here, never commit

# === LibreChat ===
LIBRECHAT_JWT_SECRET=ChangeMe123!LibreChat
LIBRECHAT_SERVICE_JWT=                      # long-lived JWT for service account

# === API ===
API_JWT_SECRET=ChangeMe123!BlueberryAPI
API_JWT_ISSUER=blueberry-finance
API_JWT_AUDIENCE=blueberry-clients

# === WhatsApp ===
WHATSAPP_VERIFY_TOKEN=
WHATSAPP_TOKEN=

# === Seq ===
SEQ_API_KEY=
```

### 10.3 `docker-compose.yml` (skeleton — fill versions)

```yaml
# All service names follow pattern: local-dev-{service}

services:

  local-dev-postgres:
    container_name: local-dev-postgres
    image: postgres:16.3
    restart: unless-stopped
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./init-db.sql:/docker-entrypoint-initdb.d/init.sql:ro

  local-dev-rabbitmq:
    container_name: local-dev-rabbitmq
    image: rabbitmq:3.13.3-management
    restart: unless-stopped
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
      RABBITMQ_DEFAULT_VHOST: ${RABBITMQ_VHOST}
    ports:
      - "5672:5672"
      - "15672:15672"

  local-dev-minio:
    container_name: local-dev-minio
    image: minio/minio:RELEASE.2024-06-13T22-53-53Z
    restart: unless-stopped
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: ${MINIO_ROOT_USER}
      MINIO_ROOT_PASSWORD: ${MINIO_ROOT_PASSWORD}
    ports:
      - "9000:9000"
      - "9001:9001"
    volumes:
      - minio-data:/data

  local-dev-litellm:
    container_name: local-dev-litellm
    image: ghcr.io/berriai/litellm:main-v1.40.10
    restart: unless-stopped
    environment:
      LITELLM_MASTER_KEY: ${LITELLM_MASTER_KEY}
      OPENAI_API_KEY: ${OPENAI_API_KEY}
    ports:
      - "4000:4000"
    volumes:
      - ./litellm/config.yaml:/app/config.yaml:ro
    command: ["--config", "/app/config.yaml"]

  local-dev-librechat:
    container_name: local-dev-librechat
    image: ghcr.io/danny-avila/librechat:v0.7.5
    restart: unless-stopped
    environment:
      OPENAI_API_KEY: ${LITELLM_LIBRECHAT_KEY}
      OPENAI_BASE_URL: http://local-dev-litellm:4000/v1
      JWT_SECRET: ${LIBRECHAT_JWT_SECRET}
      CUSTOM_ENDPOINT_URL: http://local-dev-api:8080/api/v1.0/chat/stream
      CUSTOM_ENDPOINT_API_KEY: ${LIBRECHAT_SERVICE_JWT}
    ports:
      - "3080:3080"
    depends_on:
      - local-dev-litellm
      - local-dev-mongo

  local-dev-mongo:
    container_name: local-dev-mongo
    image: mongo:7.0.9
    restart: unless-stopped
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db

  local-dev-seq:
    container_name: local-dev-seq
    image: datalust/seq:2024.2
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      SEQ_API_KEY: ${SEQ_API_KEY}
    ports:
      - "5341:5341"
      - "8081:80"
    volumes:
      - seq-data:/data

  local-dev-portainer:
    container_name: local-dev-portainer
    image: portainer/portainer-ce:2.20.3
    restart: unless-stopped
    ports:
      - "9443:9443"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - portainer-data:/data

volumes:
  postgres-data:
  minio-data:
  mongo-data:
  seq-data:
  portainer-data:
```

### 10.4 LiteLLM `config.yaml`

```yaml
model_list:
  - model_name: gpt-4.1-mini
    litellm_params:
      model: openai/gpt-4.1-mini
      api_key: os.environ/OPENAI_API_KEY

general_settings:
  master_key: os.environ/LITELLM_MASTER_KEY

virtual_keys:
  - key: os.environ/LITELLM_LIBRECHAT_KEY
    models: ["gpt-4.1-mini"]
    max_budget: 10.0
    budget_duration: 30d
  - key: os.environ/LITELLM_API_KEY
    models: ["gpt-4.1-mini"]
    max_budget: 20.0
    budget_duration: 30d
```

---

## 11. Testing Strategy

### 11.1 General Rules

- Test project: `BlueBerryFinance.Tests` (xUnit).
- **Coverage over quantity**: 10 well-designed tests that cover edge cases beat 100 trivial happy-path tests.
- Write tests **while** implementing the feature, not after.
- Always write regression tests when fixing a bug (the test must fail first without the fix).
- Structure mirrors source: `Tests/API/Handlers/`, `Tests/API/Controllers/`, `Tests/Web/Clients/`, etc.
- Mocks live in `Tests/Data/` and are instantiated via DI (`ServiceCollection` + `BuildServiceProvider()`).
- Use real in-memory or SQLite DB for EF Core tests — never mock `DbContext`.

### 11.2 What to Test

| Layer | Test type | Notes |
|---|---|---|
| Handlers | Unit | Mock agent interfaces; use in-memory EF DB |
| Controllers | Integration | WebApplicationFactory, test status codes + response shape |
| Agents | Unit | Mock IAIAgentFactory |
| RabbitMQ consumers | Unit | Mock publisher; assert correct handler called |
| Web Clients | Unit | Mock HttpMessageHandler |
| Auth/JWT | Integration | Verify 401 on missing/expired token |

### 11.3 Mock Pattern (DI-based)

```csharp
// Tests/Data/TestServiceFactory.cs
public static IServiceProvider BuildTestServices(Action<IServiceCollection>? overrides = null)
{
    var services = new ServiceCollection();
    services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
    services.AddScoped<IFinancialAssistantAgent, MockFinancialAssistantAgent>();
    services.AddScoped<ITransactionsHandler, RegisterTransactionHandler>();
    overrides?.Invoke(services);
    return services.BuildServiceProvider();
}

// Tests/Data/Mocks/MockFinancialAssistantAgent.cs
public class MockFinancialAssistantAgent : IFinancialAssistantAgent
{
    public Task<FinancialAnalysisResponse?> AskAsync(string prompt, AIProvider provider = AIProvider.OpenAI)
        => Task.FromResult<FinancialAnalysisResponse?>(new() { Answer = "Mock answer" });
}
```

---

## 12. Implementation Phases

Work through phases in order. Each phase must include working tests before moving to the next.

### Phase 0 — Foundation (do this first)
- [ ] configure User Secrets.
- [ ] Set up `docker-compose.yml` with PostgreSQL, RabbitMQ, MinIO, Seq.
- [ ] Add EF Core, configure `AppDbContext`, run initial migration.
- [ ] Add Serilog + Seq sink.
- [ ] Add JWT authentication middleware to API.
- [ ] Add ASP.NET Identity to Web; seed admin user.
- [ ] Add `AuditMiddleware` and `AuditLog` DB.

### Phase 1 — Core Financial Data
- [ ] Entities: `User`, `BankAccount`, `Category`, `Store`, `FixedExpense`, `Transaction`.
- [ ] CRUD endpoints for all entities with action endpoints for register/update/delete.
- [ ] Web pages: Transactions list, register transaction form, bank accounts.
- [ ] Typed HTTP clients for all contexts.

### Phase 2 — Agent & Chat
- [ ] Refactor agent system to call LiteLLM.
- [ ] Add read tools (GetTransactions, GetBankAccounts) to Financial Assistant agent.
- [ ] Add write tools with `AgentApproval` flow.
- [ ] `/chat/stream` SSE endpoint.
- [ ] LibreChat connected to API stream + LiteLLM.
- [ ] Web Chat page using the stream endpoint.

### Phase 3 — Jobs & Messaging
- [ ] RabbitMQ publisher/consumer infrastructure.
- [ ] `MonthlyReportJob`: generate PDF report and store in MinIO.
- [ ] Hangfire dashboard (or simple hosted service scheduler).
- [ ] User-triggered job endpoints: `POST /report/monthly`.

### Phase 4 — Fiscal Notes & WhatsApp
- [ ] MinIO blob storage service.
- [ ] `FiscalNoteAgent` with image input support.
- [ ] Web file upload component → `FiscalNoteAgent`.
- [ ] WhatsApp webhook receiver → publish to `finance.fiscal.note` queue.

### Phase 5 — Bank Integration
- [ ] Research and confirm bank aggregator (Pluggy for BR, Nordigen for PT).
- [ ] OAuth2 consent flow for bank connection.
- [ ] `BankSyncJob` mapping + transaction deduplication.

### Phase 6 — Bot Framework / Teams / Discord
- [ ] Bot Framework adapter.
- [ ] Teams channel connector.
- [ ] Discord connector (optional).

---

## Appendix: Security Checklist

- [ ] No secrets in source control (appsettings, docker-compose committed without values).
- [ ] HTTPS enforced in production; HSTS header set.
- [ ] CORS locked to known origins only.
- [ ] JWT `Secret` ≥ 512-bit random string.
- [ ] WhatsApp webhook signature (`X-Hub-Signature-256`) validated on every call.
- [ ] Bank OAuth tokens stored AES-256 encrypted in DB; key never in DB.
- [ ] Agent write tools require explicit user approval before executing.
- [ ] Audit log captures every API call with correlation ID, payload, response.
- [ ] Sensitive fields (passwords, tokens) redacted from audit log payload.
- [ ] Soft delete only — no physical deletes without admin confirmation.
- [ ] Rate limiting on `/auth/login` and `/whatsapp/webhook`.
- [ ] Input validation (FluentValidation) on all request objects before reaching handlers.
- [ ] `X-Content-Type-Options: nosniff` and `X-Frame-Options: DENY` headers set.
