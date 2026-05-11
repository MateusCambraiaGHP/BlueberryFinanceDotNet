# Refactor: Feature Folders, Dead Code Removal & Auth Middleware

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reorganize `Application/` into per-feature folders, delete all dead/deprecated code, rename `CsvImport` → `BankImport`, and replace `AuthController` + `AuthHandler` with a `LoginMiddleware`.

**Architecture:** Each feature gets a single folder under `Application/Features/<Feature>/` containing its interface, handler, request models, and response models. The old flat `Handlers/`, `Requests/`, and `Responses/` folders are removed entirely. Auth credential checking moves from a controller+handler pair into a dedicated ASP.NET `IMiddleware` that intercepts `POST /api/v1.0/auth/login` before the controller pipeline.

**Tech Stack:** .NET 9, ASP.NET Core, Entity Framework Core, BCrypt.Net, custom `IJwtService`

---

## File Map

### Deleted
| Path | Reason |
|---|---|
| `Application/Handlers/TransactionsHandler.cs` | Dead — no controller calls it |
| `Application/Handlers/Interfaces/ITransactionsHandler.cs` | Interface for dead handler |
| `Application/Handlers/Interfaces/IRequestHandler.cs` | Only used by dead ITransactionsHandler |
| `Application/Handlers/AuthHandler.cs` | Replaced by LoginMiddleware |
| `Application/Handlers/Interfaces/IAuthHandler.cs` | Replaced by LoginMiddleware |
| `Controllers/AuthController.cs` | Replaced by LoginMiddleware |
| `Controllers/OriginTypeController.cs` | File body is just a comment |
| `Application/Requests/FinancialAnalysisRequest.cs` | Used only by dead TransactionsHandler |
| `Application/Responses/FinancialAnalysisResponse.cs` | Used only by dead TransactionsHandler |
| `Application/Responses/ClassificationResult.cs` | Verify unused, then delete |
| `Application/Responses/SecurityValidationResult.cs` | Verify unused, then delete |
| `Application/Requests/BankConnection/` | Empty folder |
| `BlueBerryFinance.Commom/` | Empty misspelled project (obj/ only) |

### Renamed (handler + interface + controller)
| Old | New |
|---|---|
| `CsvImportHandler.cs` | `BankImportHandler.cs` |
| `ICsvImportHandler.cs` | `IBankImportHandler.cs` |
| `CsvImportController.cs` | `BankImportController.cs` |

### Created
| Path | Purpose |
|---|---|
| `Infrastructure/Middleware/LoginMiddleware.cs` | Handles `POST /api/v1.0/auth/login` |
| `Application/Features/Auth/LoginRequest.cs` | Moved from `Requests/Auth/` |
| `Application/Features/Auth/LoginResponse.cs` | Moved from `Responses/Auth/` |
| `Application/Features/AgentApproval/IAgentApprovalHandler.cs` | Moved |
| `Application/Features/AgentApproval/AgentApprovalHandler.cs` | Moved |
| `Application/Features/BankAccount/IBankAccountHandler.cs` | Moved |
| `Application/Features/BankAccount/BankAccountHandler.cs` | Moved |
| `Application/Features/BankAccount/RegisterBankAccountRequest.cs` | Moved |
| `Application/Features/BankImport/IBankImportHandler.cs` | Renamed + moved |
| `Application/Features/BankImport/BankImportHandler.cs` | Renamed + moved |
| `Application/Features/Category/ICategoryHandler.cs` | Moved |
| `Application/Features/Category/CategoryHandler.cs` | Moved |
| `Application/Features/Category/RegisterCategoryRequest.cs` | Moved |
| `Application/Features/Chat/IChatHandler.cs` | Moved |
| `Application/Features/Chat/ChatHandler.cs` | Moved |
| `Application/Features/Chat/ChatStreamRequest.cs` | Renamed from FinancialAnalysisRequest, Chat-scoped only |
| `Application/Features/Currency/ICurrencyHandler.cs` | Moved |
| `Application/Features/Currency/CurrencyHandler.cs` | Moved |
| `Application/Features/FixedExpense/IFixedExpenseHandler.cs` | Moved |
| `Application/Features/FixedExpense/FixedExpenseHandler.cs` | Moved |
| `Application/Features/FixedExpense/RegisterFixedExpenseRequest.cs` | Moved |
| `Application/Features/Report/IReportHandler.cs` | Moved |
| `Application/Features/Report/ReportHandler.cs` | Moved |
| `Application/Features/Store/IStoreHandler.cs` | Moved |
| `Application/Features/Store/StoreHandler.cs` | Moved |
| `Application/Features/Store/RegisterStoreRequest.cs` | Moved |
| `Application/Features/Transaction/ITransactionHandler.cs` | Moved |
| `Application/Features/Transaction/TransactionHandler.cs` | Moved |
| `Application/Features/Transaction/ListTransactionsRequest.cs` | Moved |
| `Application/Features/Transaction/RegisterTransactionRequest.cs` | Moved |
| `Application/Features/Transaction/UpdateTransactionRequest.cs` | Moved |

### Modified
| Path | Change |
|---|---|
| `Controllers/ChatController.cs` | Remove LibreChat endpoints, `ValidateServiceKey`, `OpenAIChatCompletionsRequest` usage |
| `Controllers/BankImportController.cs` | Rename from CsvImport, update route + injected interface |
| All controllers | Update `using` namespaces to `Features.*` |
| `Program.cs` | Remove `IAuthHandler`/`ITransactionsHandler` DI, add `LoginMiddleware`, update `ICsvImportHandler → IBankImportHandler` |

---

## Task 1: Delete dead handlers and interfaces

**Files:**
- Delete: `Application/Handlers/TransactionsHandler.cs`
- Delete: `Application/Handlers/Interfaces/ITransactionsHandler.cs`
- Delete: `Application/Handlers/Interfaces/IRequestHandler.cs`
- Delete: `Application/Requests/FinancialAnalysisRequest.cs`
- Delete: `Application/Responses/FinancialAnalysisResponse.cs`
- Delete: `Controllers/OriginTypeController.cs`
- Delete: `Application/Requests/BankConnection/` (folder)

- [ ] **Step 1: Verify nothing references these files**

  Run in the `BlueBerryFinance.API` project folder:
  ```powershell
  Select-String -Path "**\*.cs" -Pattern "ITransactionsHandler|TransactionsHandler|IRequestHandler|FinancialAnalysisRequest|FinancialAnalysisResponse|OriginTypeController" -Recurse | Where-Object { $_.Filename -notmatch "TransactionsHandler|FinancialAnalysis|OriginType" }
  ```
  Expected: zero results (no outside references).

- [ ] **Step 2: Delete dead handler and interface files**

  ```powershell
  Remove-Item "BlueBerryFinance.API\Application\Handlers\TransactionsHandler.cs"
  Remove-Item "BlueBerryFinance.API\Application\Handlers\Interfaces\ITransactionsHandler.cs"
  Remove-Item "BlueBerryFinance.API\Application\Handlers\Interfaces\IRequestHandler.cs"
  Remove-Item "BlueBerryFinance.API\Application\Requests\FinancialAnalysisRequest.cs"
  Remove-Item "BlueBerryFinance.API\Application\Responses\FinancialAnalysisResponse.cs"
  Remove-Item "BlueBerryFinance.API\Controllers\OriginTypeController.cs"
  Remove-Item -Recurse "BlueBerryFinance.API\Application\Requests\BankConnection"
  ```

- [ ] **Step 3: Verify and delete ClassificationResult + SecurityValidationResult**

  ```powershell
  Select-String -Path "**\*.cs" -Pattern "ClassificationResult|SecurityValidationResult" -Recurse | Select-Object Filename, LineNumber, Line
  ```
  If only referenced inside agent infrastructure files (not controllers/handlers as return types consumed externally), delete them:
  ```powershell
  Remove-Item "BlueBerryFinance.API\Application\Responses\ClassificationResult.cs"
  Remove-Item "BlueBerryFinance.API\Application\Responses\SecurityValidationResult.cs"
  ```
  If they ARE referenced externally, leave them — they will be moved to the appropriate feature folder in a later task.

- [ ] **Step 4: Remove ITransactionsHandler DI registration from Program.cs**

  In `Program.cs`, find and delete this line:
  ```csharp
  builder.Services.AddScoped<ITransactionsHandler, TransactionsHandler>();
  ```
  Also remove any `using` that references the deleted types.

- [ ] **Step 5: Build to confirm no compile errors**

  ```powershell
  dotnet build BlueBerryFinance.API\BlueBerryFinance.API.csproj
  ```
  Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Commit**

  ```bash
  git add -A
  git commit -m "chore: delete dead handlers, interfaces, and empty files"
  ```

---

## Task 2: Remove LibreChat endpoints from ChatController

**Files:**
- Modify: `Controllers/ChatController.cs`
- Delete: `Application/Requests/Chat/OpenAIChatCompletionsRequest.cs`

The following must be removed from `ChatController.cs`:
- `GET /v1/models` and `GET /models` action (`GetModels`)
- `POST chat/completions`, `POST v1/chat/completions`, `POST /chat/completions`, `POST /v1/chat/completions` action (`ChatCompletions`)
- `ValidateServiceKey()` private method
- `IConfiguration _config` field and constructor parameter
- The `using` for `OpenAIChatCompletionsRequest` and `System.Security.Claims`
- The `[AllowAnonymous]` attributes that only covered removed actions (keep the class-level `[Authorize]`)

- [ ] **Step 1: Edit ChatController — remove LibreChat members**

  The resulting `ChatController.cs` should look like this:

  ```csharp
  using BlueBerryFinance.API.Application.Features.Chat;
  using BlueBerryFinance.API.Application.Handlers.Interfaces;
  using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  namespace BlueBerryFinance.API.Controllers
  {
      [Authorize(Policy = "UserOrAdmin")]
      public class ChatController : BaseController
      {
          private readonly IChatHandler _chatHandler;
          private readonly IMinioService _minio;
          private readonly ILogger<ChatController> _logger;

          private static readonly JsonSerializerOptions _jsonOptions = new()
          {
              DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
              PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
          };

          private static readonly string[] _allowedImageTypes =
              ["image/jpeg", "image/png", "image/gif", "image/webp"];

          public ChatController(
              IChatHandler chatHandler,
              IMinioService minio,
              ILogger<ChatController> logger)
          {
              _chatHandler = chatHandler;
              _minio = minio;
              _logger = logger;
          }

          [HttpPost("chat/upload-image")]
          [Consumes("multipart/form-data")]
          [ProducesResponseType(StatusCodes.Status200OK)]
          [ProducesResponseType(StatusCodes.Status400BadRequest)]
          public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken ct)
          {
              if (file is null || file.Length == 0)
                  return BadRequest(new { error = "No file provided." });

              if (!_allowedImageTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                  return BadRequest(new { error = "Only image files are accepted (JPEG, PNG, GIF, WebP)." });

              if (file.Length > 10 * 1024 * 1024)
                  return BadRequest(new { error = "Image size must not exceed 10 MB." });

              try
              {
                  await using var stream = file.OpenReadStream();
                  var url = await _minio.UploadAsync(stream, file.FileName, file.ContentType, ct);
                  return Ok(new { url });
              }
              catch (Exception ex)
              {
                  return HandleException(ex, _logger);
              }
          }

          [HttpPost("chat/stream")]
          public async Task StreamChat([FromBody] ChatStreamRequest request, CancellationToken ct)
          {
              Response.Headers["Content-Type"] = "text/event-stream";
              Response.Headers["Cache-Control"] = "no-cache";
              Response.Headers["X-Accel-Buffering"] = "no";

              try
              {
                  await foreach (var chunk in _chatHandler.StreamAsync(request.Prompt, includeTools: true, ct))
                  {
                      var data = JsonSerializer.Serialize(new { content = chunk });
                      await Response.WriteAsync($"data: {data}\n\n", ct);
                      await Response.Body.FlushAsync(ct);
                  }
              }
              catch (OperationCanceledException) { }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error during chat stream for user {UserId}", CurrentUserId);
                  var error = JsonSerializer.Serialize(new { error = "An error occurred during streaming." });
                  await Response.WriteAsync($"data: {error}\n\n", ct);
                  await Response.Body.FlushAsync(ct);
              }
              finally
              {
                  if (!ct.IsCancellationRequested)
                  {
                      await Response.WriteAsync("data: [DONE]\n\n", ct);
                      await Response.Body.FlushAsync(ct);
                  }
              }
          }
      }
  }
  ```

  Note: `ChatStreamRequest` will be created in Task 4. For now the `using` can reference the old `FinancialAnalysisRequest` temporarily — it will be fixed in Task 4.

- [ ] **Step 2: Delete OpenAIChatCompletionsRequest.cs**

  ```powershell
  Remove-Item "BlueBerryFinance.API\Application\Requests\Chat\OpenAIChatCompletionsRequest.cs"
  ```

- [ ] **Step 3: Build**

  ```powershell
  dotnet build BlueBerryFinance.API\BlueBerryFinance.API.csproj
  ```
  Expected: 0 errors.

- [ ] **Step 4: Commit**

  ```bash
  git add -A
  git commit -m "feat: remove LibreChat-compatible endpoints from ChatController"
  ```

---

## Task 3: Rename CsvImport → BankImport

**Files:**
- Rename handler, interface, and controller.

- [ ] **Step 1: Create BankImportHandler.cs**

  Create `BlueBerryFinance.API\Application\Handlers\BankImportHandler.cs` — copy content from `CsvImportHandler.cs` with:
  - Class name: `BankImportHandler`
  - Namespace: `BlueBerryFinance.API.Application.Handlers`
  - Implements: `IBankImportHandler`
  - Logger type: `ILogger<BankImportHandler>`
  - All internal logic unchanged

- [ ] **Step 2: Create IBankImportHandler.cs**

  Create `BlueBerryFinance.API\Application\Handlers\Interfaces\IBankImportHandler.cs`:

  ```csharp
  using BlueBerryFinance.Common.ViewModels;

  namespace BlueBerryFinance.API.Application.Handlers.Interfaces
  {
      public interface IBankImportHandler
      {
          /// <summary>
          /// Parses a bank CSV export and saves new transactions to the database.
          /// Skips rows that already exist (same date + amount + description on the same account).
          /// </summary>
          Task<CsvImportResultViewModel> ImportAsync(
              Guid bankAccountId,
              Guid userId,
              Stream csvStream,
              CancellationToken ct = default);
      }
  }
  ```

- [ ] **Step 3: Create BankImportController.cs**

  Create `BlueBerryFinance.API\Controllers\BankImportController.cs`:

  ```csharp
  using BlueBerryFinance.API.Application.Handlers.Interfaces;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  namespace BlueBerryFinance.API.Controllers
  {
      [Route("api/v1.0/bank-import")]
      [Authorize]
      public class BankImportController : BaseController
      {
          private readonly IBankImportHandler _handler;
          private readonly ILogger<BankImportController> _logger;

          public BankImportController(IBankImportHandler handler, ILogger<BankImportController> logger)
          {
              _handler = handler;
              _logger = logger;
          }

          /// <summary>
          /// Upload a CSV bank statement export and import transactions into the given bank account.
          /// Supported formats: ActivoBank, CGD, BPI, Millennium BCP (semicolon-separated Portuguese CSV).
          /// Duplicate rows (same date + amount + description) are automatically skipped.
          /// </summary>
          [HttpPost("{bankAccountId:guid}")]
          [Consumes("multipart/form-data")]
          [ProducesResponseType(StatusCodes.Status200OK)]
          [ProducesResponseType(StatusCodes.Status400BadRequest)]
          [ProducesResponseType(StatusCodes.Status404NotFound)]
          public async Task<IActionResult> Import(Guid bankAccountId, IFormFile file, CancellationToken ct)
          {
              if (file is null || file.Length == 0)
                  return BadRequest(new { error = "No file provided." });

              if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) &&
                  !file.ContentType.Contains("text"))
                  return BadRequest(new { error = "Only CSV files are accepted." });

              try
              {
                  await using var stream = file.OpenReadStream();
                  var result = await _handler.ImportAsync(bankAccountId, CurrentUserId, stream, ct);
                  return Ok(result);
              }
              catch (KeyNotFoundException ex)
              {
                  return NotFound(new { error = ex.Message });
              }
              catch (Exception ex)
              {
                  return HandleException(ex, _logger);
              }
          }
      }
  }
  ```

- [ ] **Step 4: Update Program.cs DI registration**

  Replace:
  ```csharp
  builder.Services.AddScoped<ICsvImportHandler, CsvImportHandler>();
  ```
  With:
  ```csharp
  builder.Services.AddScoped<IBankImportHandler, BankImportHandler>();
  ```

- [ ] **Step 5: Delete old Csv files**

  ```powershell
  Remove-Item "BlueBerryFinance.API\Application\Handlers\CsvImportHandler.cs"
  Remove-Item "BlueBerryFinance.API\Application\Handlers\Interfaces\ICsvImportHandler.cs"
  Remove-Item "BlueBerryFinance.API\Controllers\CsvImportController.cs"
  ```

- [ ] **Step 6: Build**

  ```powershell
  dotnet build BlueBerryFinance.API\BlueBerryFinance.API.csproj
  ```
  Expected: 0 errors.

- [ ] **Step 7: Commit**

  ```bash
  git add -A
  git commit -m "refactor: rename CsvImport to BankImport (handler, interface, controller, route)"
  ```

---

## Task 4: Create feature folders and move all handlers + models

This is the largest task. For each feature, we: create the folder, copy the files with updated namespaces, then delete the originals. All at once at the end to avoid a broken intermediate state.

**Target structure:**
```
Application/Features/
  AgentApproval/   IAgentApprovalHandler.cs  AgentApprovalHandler.cs
  Auth/            LoginRequest.cs  LoginResponse.cs
  BankAccount/     IBankAccountHandler.cs  BankAccountHandler.cs  RegisterBankAccountRequest.cs
  BankImport/      IBankImportHandler.cs  BankImportHandler.cs
  Category/        ICategoryHandler.cs  CategoryHandler.cs  RegisterCategoryRequest.cs
  Chat/            IChatHandler.cs  ChatHandler.cs  ChatStreamRequest.cs
  Currency/        ICurrencyHandler.cs  CurrencyHandler.cs
  FixedExpense/    IFixedExpenseHandler.cs  FixedExpenseHandler.cs  RegisterFixedExpenseRequest.cs
  Report/          IReportHandler.cs  ReportHandler.cs
  Store/           IStoreHandler.cs  StoreHandler.cs  RegisterStoreRequest.cs
  Transaction/     ITransactionHandler.cs  TransactionHandler.cs  ListTransactionsRequest.cs
                   RegisterTransactionRequest.cs  UpdateTransactionRequest.cs
```

**Namespace mapping:**

| Old namespace | New namespace |
|---|---|
| `BlueBerryFinance.API.Application.Handlers` | `BlueBerryFinance.API.Application.Features.<Feature>` |
| `BlueBerryFinance.API.Application.Handlers.Interfaces` | `BlueBerryFinance.API.Application.Features.<Feature>` |
| `BlueBerryFinance.API.Application.Requests.Auth` | `BlueBerryFinance.API.Application.Features.Auth` |
| `BlueBerryFinance.API.Application.Requests.BankAccount` | `BlueBerryFinance.API.Application.Features.BankAccount` |
| `BlueBerryFinance.API.Application.Requests.Category` | `BlueBerryFinance.API.Application.Features.Category` |
| `BlueBerryFinance.API.Application.Requests.Store` | `BlueBerryFinance.API.Application.Features.Store` |
| `BlueBerryFinance.API.Application.Requests.FixedExpense` | `BlueBerryFinance.API.Application.Features.FixedExpense` |
| `BlueBerryFinance.API.Application.Requests.Transaction` | `BlueBerryFinance.API.Application.Features.Transaction` |
| `BlueBerryFinance.API.Application.Requests` (FinancialAnalysisRequest) | Becomes `ChatStreamRequest` in `BlueBerryFinance.API.Application.Features.Chat` |
| `BlueBerryFinance.API.Application.Responses.Auth` | `BlueBerryFinance.API.Application.Features.Auth` |

- [ ] **Step 1: Create all feature directories**

  ```powershell
  $base = "BlueBerryFinance.API\Application\Features"
  @("AgentApproval","Auth","BankAccount","BankImport","Category","Chat","Currency","FixedExpense","Report","Store","Transaction") | ForEach-Object {
      New-Item -ItemType Directory -Path "$base\$_" -Force
  }
  ```

- [ ] **Step 2: Create AgentApproval feature files**

  Copy `Application/Handlers/Interfaces/IAgentApprovalHandler.cs` → `Application/Features/AgentApproval/IAgentApprovalHandler.cs`
  Update namespace to `BlueBerryFinance.API.Application.Features.AgentApproval`.

  Copy `Application/Handlers/AgentApprovalHandler.cs` → `Application/Features/AgentApproval/AgentApprovalHandler.cs`
  Update:
  - `namespace BlueBerryFinance.API.Application.Features.AgentApproval`
  - `using BlueBerryFinance.API.Application.Features.AgentApproval;` (for the interface)
  - `using BlueBerryFinance.API.Application.Features.Transaction;` (for `RegisterTransactionRequest`)

- [ ] **Step 3: Create Auth feature files**

  Create `Application/Features/Auth/LoginRequest.cs`:
  ```csharp
  namespace BlueBerryFinance.API.Application.Features.Auth
  {
      public class LoginRequest
      {
          public string Email { get; set; } = string.Empty;
          public string Password { get; set; } = string.Empty;
      }
  }
  ```

  Create `Application/Features/Auth/LoginResponse.cs`:
  ```csharp
  namespace BlueBerryFinance.API.Application.Features.Auth
  {
      public class LoginResponse
      {
          public string Token { get; set; } = string.Empty;
          public string Email { get; set; } = string.Empty;
          public string Name { get; set; } = string.Empty;
          public string Profile { get; set; } = string.Empty;
          public DateTime ExpiresAt { get; set; }
      }
  }
  ```

- [ ] **Step 4: Create BankAccount feature files**

  Copy `Handlers/Interfaces/IBankAccountHandler.cs` → `Features/BankAccount/IBankAccountHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.BankAccount`

  Copy `Handlers/BankAccountHandler.cs` → `Features/BankAccount/BankAccountHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.BankAccount`
  Update using for `IBankAccountHandler` to same namespace (remove separate Interfaces using).

  Copy `Requests/BankAccount/RegisterBankAccountRequest.cs` → `Features/BankAccount/RegisterBankAccountRequest.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.BankAccount`

- [ ] **Step 5: Create BankImport feature files**

  Copy `Handlers/Interfaces/IBankImportHandler.cs` → `Features/BankImport/IBankImportHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.BankImport`

  Copy `Handlers/BankImportHandler.cs` → `Features/BankImport/BankImportHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.BankImport`
  Update `using` for `IBankImportHandler` to same namespace.

- [ ] **Step 6: Create Category feature files**

  Copy `Handlers/Interfaces/ICategoryHandler.cs` → `Features/Category/ICategoryHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Category`

  Copy `Handlers/CategoryHandler.cs` → `Features/Category/CategoryHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Category`

  Copy `Requests/Category/RegisterCategoryRequest.cs` → `Features/Category/RegisterCategoryRequest.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Category`

- [ ] **Step 7: Create Chat feature files**

  Copy `Handlers/Interfaces/IChatHandler.cs` → `Features/Chat/IChatHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Chat`

  Copy `Handlers/ChatHandler.cs` → `Features/Chat/ChatHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Chat`
  Update using for `IChatHandler` to same namespace.

  Create `Features/Chat/ChatStreamRequest.cs` (replaces the old `FinancialAnalysisRequest`):
  ```csharp
  namespace BlueBerryFinance.API.Application.Features.Chat
  {
      public class ChatStreamRequest
      {
          public string Prompt { get; set; } = string.Empty;
      }
  }
  ```

- [ ] **Step 8: Create Currency feature files**

  Copy `Handlers/Interfaces/ICurrencyHandler.cs` → `Features/Currency/ICurrencyHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Currency`

  Copy `Handlers/CurrencyHandler.cs` → `Features/Currency/CurrencyHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Currency`

- [ ] **Step 9: Create FixedExpense feature files**

  Copy `Handlers/Interfaces/IFixedExpenseHandler.cs` → `Features/FixedExpense/IFixedExpenseHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.FixedExpense`

  Copy `Handlers/FixedExpenseHandler.cs` → `Features/FixedExpense/FixedExpenseHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.FixedExpense`

  Copy `Requests/FixedExpense/RegisterFixedExpenseRequest.cs` → `Features/FixedExpense/RegisterFixedExpenseRequest.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.FixedExpense`

- [ ] **Step 10: Create Report feature files**

  Copy `Handlers/Interfaces/IReportHandler.cs` → `Features/Report/IReportHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Report`

  Copy `Handlers/ReportHandler.cs` → `Features/Report/ReportHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Report`

- [ ] **Step 11: Create Store feature files**

  Copy `Handlers/Interfaces/IStoreHandler.cs` → `Features/Store/IStoreHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Store`

  Copy `Handlers/StoreHandler.cs` → `Features/Store/StoreHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Store`

  Copy `Requests/Store/RegisterStoreRequest.cs` → `Features/Store/RegisterStoreRequest.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Store`

- [ ] **Step 12: Create Transaction feature files**

  Copy `Handlers/Interfaces/ITransactionHandler.cs` → `Features/Transaction/ITransactionHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Transaction`

  Copy `Handlers/TransactionHandler.cs` → `Features/Transaction/TransactionHandler.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Transaction`

  Copy `Requests/Transaction/ListTransactionsRequest.cs` → `Features/Transaction/ListTransactionsRequest.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Transaction`

  Copy `Requests/Transaction/RegisterTransactionRequest.cs` → `Features/Transaction/RegisterTransactionRequest.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Transaction`

  Copy `Requests/Transaction/UpdateTransactionRequest.cs` → `Features/Transaction/UpdateTransactionRequest.cs`
  Namespace: `BlueBerryFinance.API.Application.Features.Transaction`

- [ ] **Step 13: Update all controllers with new namespaces**

  For each controller, replace old `using` statements with new `Features.*` namespaces:

  `AgentApprovalController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.AgentApproval;
  ```

  `BankAccountController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.BankAccount;
  ```

  `BankImportController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.BankImport;
  ```

  `CategoryController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.Category;
  ```

  `ChatController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.Chat;
  ```

  `CurrencyController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.Currency;
  ```

  `FixedExpenseController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.FixedExpense;
  ```

  `ReportController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.Report;
  ```

  `StoreController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.Store;
  ```

  `TransactionController.cs`:
  ```csharp
  using BlueBerryFinance.API.Application.Features.Transaction;
  ```

- [ ] **Step 14: Update Program.cs usings and DI registrations**

  Remove all old `using BlueBerryFinance.API.Application.Handlers*` lines.
  Add:
  ```csharp
  using BlueBerryFinance.API.Application.Features.AgentApproval;
  using BlueBerryFinance.API.Application.Features.BankAccount;
  using BlueBerryFinance.API.Application.Features.BankImport;
  using BlueBerryFinance.API.Application.Features.Category;
  using BlueBerryFinance.API.Application.Features.Chat;
  using BlueBerryFinance.API.Application.Features.Currency;
  using BlueBerryFinance.API.Application.Features.FixedExpense;
  using BlueBerryFinance.API.Application.Features.Report;
  using BlueBerryFinance.API.Application.Features.Store;
  using BlueBerryFinance.API.Application.Features.Transaction;
  ```

  DI registrations — replace all handler registrations (the `// ── Application handlers ──` block):
  ```csharp
  // ── Application handlers ──────────────────────────────────────────────────
  builder.Services.AddScoped<IAgentApprovalHandler, AgentApprovalHandler>();
  builder.Services.AddScoped<IChatHandler, ChatHandler>();
  builder.Services.AddScoped<ITransactionHandler, TransactionHandler>();
  builder.Services.AddScoped<IBankAccountHandler, BankAccountHandler>();
  builder.Services.AddScoped<ICategoryHandler, CategoryHandler>();
  builder.Services.AddScoped<IStoreHandler, StoreHandler>();
  builder.Services.AddScoped<IFixedExpenseHandler, FixedExpenseHandler>();
  builder.Services.AddScoped<ICurrencyHandler, CurrencyHandler>();
  builder.Services.AddScoped<IReportHandler, ReportHandler>();
  builder.Services.AddScoped<IBankImportHandler, BankImportHandler>();
  ```
  Note: `IAuthHandler` is intentionally absent — replaced by middleware in Task 5.

- [ ] **Step 15: Build**

  ```powershell
  dotnet build BlueBerryFinance.API\BlueBerryFinance.API.csproj
  ```
  Expected: 0 errors. If there are errors about missing types, check usings in the file that fails.

- [ ] **Step 16: Delete old Handlers/, Requests/, Responses/ folders**

  Only do this after the build passes in Step 15.

  ```powershell
  Remove-Item -Recurse "BlueBerryFinance.API\Application\Handlers"
  Remove-Item -Recurse "BlueBerryFinance.API\Application\Requests"
  Remove-Item -Recurse "BlueBerryFinance.API\Application\Responses"
  Remove-Item -Recurse "BlueBerryFinance.API\Application\ViewModels" # if empty
  ```

- [ ] **Step 17: Build again to confirm nothing broke**

  ```powershell
  dotnet build BlueBerryFinance.API\BlueBerryFinance.API.csproj
  ```
  Expected: 0 errors.

- [ ] **Step 18: Commit**

  ```bash
  git add -A
  git commit -m "refactor: reorganize Application into Features/<feature> folder structure"
  ```

---

## Task 5: Replace AuthHandler + AuthController with LoginMiddleware

**Files:**
- Create: `Infrastructure/Middleware/LoginMiddleware.cs`
- Modify: `Program.cs` (register middleware, remove IAuthHandler DI)
- The `Application/Features/Auth/LoginRequest.cs` and `LoginResponse.cs` created in Task 4 are used here.

- [ ] **Step 1: Create LoginMiddleware.cs**

  Create `BlueBerryFinance.API\Infrastructure\Middleware\LoginMiddleware.cs`:

  ```csharp
  using BlueBerryFinance.API.Application.Features.Auth;
  using BlueBerryFinance.API.Data.Context;
  using BlueBerryFinance.API.Infrastructure.Models;
  using BlueBerryFinance.API.Infrastructure.Services.Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Options;
  using System.Text.Json;

  namespace BlueBerryFinance.API.Infrastructure.Middleware
  {
      /// <summary>
      /// Intercepts POST /api/v1.0/auth/login before the controller pipeline.
      /// Validates credentials, issues a JWT, and writes the LoginResponse directly.
      /// All other requests pass through unchanged.
      /// </summary>
      public class LoginMiddleware : IMiddleware
      {
          private const string LoginPath = "/api/v1.0/auth/login";

          private static readonly JsonSerializerOptions _jsonOptions = new()
          {
              PropertyNamingPolicy = JsonNamingPolicy.CamelCase
          };

          private readonly AppDbContext _db;
          private readonly IJwtService _jwtService;
          private readonly JwtOptions _jwtOptions;
          private readonly ILogger<LoginMiddleware> _logger;

          public LoginMiddleware(
              AppDbContext db,
              IJwtService jwtService,
              IOptions<JwtOptions> jwtOptions,
              ILogger<LoginMiddleware> logger)
          {
              _db = db;
              _jwtService = jwtService;
              _jwtOptions = jwtOptions.Value;
              _logger = logger;
          }

          public async Task InvokeAsync(HttpContext context, RequestDelegate next)
          {
              if (!context.Request.Path.Equals(LoginPath, StringComparison.OrdinalIgnoreCase)
                  || !HttpMethods.IsPost(context.Request.Method))
              {
                  await next(context);
                  return;
              }

              LoginRequest? request;
              try
              {
                  request = await JsonSerializer.DeserializeAsync<LoginRequest>(
                      context.Request.Body, _jsonOptions);
              }
              catch
              {
                  context.Response.StatusCode = StatusCodes.Status400BadRequest;
                  await context.Response.WriteAsync("Invalid request body.");
                  return;
              }

              if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
              {
                  context.Response.StatusCode = StatusCodes.Status400BadRequest;
                  await context.Response.WriteAsync("Email and password are required.");
                  return;
              }

              var user = await _db.Users
                  .AsNoTracking()
                  .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

              if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
              {
                  context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                  await context.Response.WriteAsJsonAsync(new { message = "Invalid credentials." });
                  return;
              }

              var token = _jwtService.GenerateToken(user);

              var response = new LoginResponse
              {
                  Token = token,
                  Email = user.Email,
                  Name = user.Name,
                  Profile = user.Profile,
                  ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.ExpiryDays)
              };

              context.Response.StatusCode = StatusCodes.Status200OK;
              context.Response.ContentType = "application/json";
              await JsonSerializer.SerializeAsync(context.Response.Body, response, _jsonOptions);
          }
      }
  }
  ```

- [ ] **Step 2: Register LoginMiddleware in Program.cs**

  Add DI registration (as scoped, since it uses scoped `AppDbContext`):
  ```csharp
  builder.Services.AddScoped<LoginMiddleware>();
  ```

  In the middleware pipeline, add **before** `app.UseAuthentication()`:
  ```csharp
  app.UseMiddleware<LoginMiddleware>();
  app.UseAuthentication();
  app.UseAuthorization();
  ```

- [ ] **Step 3: Build**

  ```powershell
  dotnet build BlueBerryFinance.API\BlueBerryFinance.API.csproj
  ```
  Expected: 0 errors.

- [ ] **Step 4: Manual smoke test**

  Start the API and run:
  ```powershell
  # Should return 200 with token
  Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/v1.0/auth/login" `
    -ContentType "application/json" `
    -Body '{"email":"mateus@hotmail.com","password":"ChangeMe123!"}'

  # Should return 401
  Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/v1.0/auth/login" `
    -ContentType "application/json" `
    -Body '{"email":"wrong@test.com","password":"wrong"}'
  ```

- [ ] **Step 5: Commit**

  ```bash
  git add -A
  git commit -m "feat: replace AuthController+AuthHandler with LoginMiddleware"
  ```

---

## Task 6: Delete remaining old project artifacts

- [ ] **Step 1: Verify Commom project is not referenced anywhere**

  ```powershell
  Select-String -Path "**\*.csproj" -Pattern "Commom" -Recurse
  Select-String -Path "**\*.cs" -Pattern "Commom" -Recurse
  ```
  Expected: 0 results.

- [ ] **Step 2: Remove Commom project folder**

  ```powershell
  Remove-Item -Recurse "BlueBerryFinance.Commom"
  ```

- [ ] **Step 3: Remove Commom from solution if listed**

  Open `BlueBerryFinance.slnx` and remove the `BlueBerryFinance.Commom` project entry if present.

- [ ] **Step 4: Final build of entire solution**

  ```powershell
  dotnet build BlueBerryFinance.slnx
  ```
  Expected: 0 errors, 0 warnings about missing projects.

- [ ] **Step 5: Commit**

  ```bash
  git add -A
  git commit -m "chore: remove empty misspelled Commom project"
  ```

---

## Self-Review

**Spec coverage:**
- [x] Dead handlers deleted (`TransactionsHandler`, `ITransactionsHandler`, `IRequestHandler`) — Task 1
- [x] LibreChat endpoints removed — Task 2
- [x] CsvImport → BankImport rename — Task 3
- [x] Feature folder reorganization — Task 4
- [x] Auth → middleware — Task 5
- [x] Empty Commom project deleted — Task 6
- [x] `OriginTypeController` deleted — Task 1
- [x] `OpenAIChatCompletionsRequest` deleted — Task 2
- [x] All controllers updated with new namespaces — Task 4 Step 13
- [x] Program.cs updated — Tasks 1, 3, 4, 5

**No placeholders present.**

**Type consistency:** All interfaces, implementations, and DI registrations use the same type names. `ChatStreamRequest` replaces `FinancialAnalysisRequest` consistently in both the controller and the new feature folder.
