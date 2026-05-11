# Controller Refactor + FluentValidation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Consolidate GET/GetById into a single filtered GET per controller, move AI response classes into their feature folders, remove XML summary comments from controllers, and replace all manual null/length validation with FluentValidation validators.

**Architecture:** Vertical-slice feature folders. Each feature owns its own request, filter, validator, and (where applicable) response classes. FluentValidation auto-validation fires before the controller action runs, returning 400 with structured errors automatically. Handlers are updated to accept unified filter requests instead of separate list/getById signatures.

**Tech Stack:** .NET 10, ASP.NET Core, FluentValidation.AspNetCore v11, Entity Framework Core 9

---

## File Map

### Files to Create

| Path | Purpose |
|---|---|
| `Application/Features/BankAccount/BankAccountFilterRequest.cs` | Filter with optional `Id?` |
| `Application/Features/BankAccount/RegisterBankAccountRequestValidator.cs` | FluentValidation validator |
| `Application/Features/Category/CategoryFilterRequest.cs` | Filter with optional `Id?` |
| `Application/Features/Category/RegisterCategoryRequestValidator.cs` | FluentValidation validator |
| `Application/Features/Store/StoreFilterRequest.cs` | Filter with optional `Id?` |
| `Application/Features/Store/RegisterStoreRequestValidator.cs` | FluentValidation validator |
| `Application/Features/FixedExpense/FixedExpenseFilterRequest.cs` | Filter with optional `Id?` |
| `Application/Features/FixedExpense/RegisterFixedExpenseRequestValidator.cs` | FluentValidation validator |
| `Application/Features/Transaction/RegisterTransactionRequestValidator.cs` | FluentValidation validator |
| `Application/Features/Transaction/UpdateTransactionRequestValidator.cs` | FluentValidation validator |
| `Application/Features/Chat/RegisterChatRequestValidator.cs` | FluentValidation validator for `ChatStreamRequest` |
| `Application/Features/Report/GetMonthlyReportRequest.cs` | Replaces inline year/month params |
| `Application/Features/Report/GetMonthlyReportRequestValidator.cs` | FluentValidation validator |
| `Application/Features/AgentApproval/SecurityValidationResult.cs` | Moved from `Application/Responses/` |
| `Application/Features/Chat/FinancialAnalysisResponse.cs` | Moved from `Application/Responses/` |
| `Application/Features/BankImport/ClassificationResult.cs` | Moved from `Application/Responses/` |

### Files to Modify

| Path | Changes |
|---|---|
| `Application/Features/BankAccount/IBankAccountHandler.cs` | Replace `ListAsync` + `GetByIdAsync` with `GetAsync(BankAccountFilterRequest)` |
| `Application/Features/BankAccount/BankAccountHandler.cs` | Implement unified `GetAsync` |
| `Application/Features/Category/ICategoryHandler.cs` | Replace `ListAsync` + `GetByIdAsync` with `GetAsync(CategoryFilterRequest)` |
| `Application/Features/Category/CategoryHandler.cs` | Implement unified `GetAsync` |
| `Application/Features/Store/IStoreHandler.cs` | Replace `ListAsync` + `GetByIdAsync` with `GetAsync(StoreFilterRequest)` |
| `Application/Features/Store/StoreHandler.cs` | Implement unified `GetAsync` |
| `Application/Features/FixedExpense/IFixedExpenseHandler.cs` | Replace `ListAsync` + `GetByIdAsync` with `GetAsync(FixedExpenseFilterRequest)` |
| `Application/Features/FixedExpense/FixedExpenseHandler.cs` | Implement unified `GetAsync` |
| `Application/Features/Transaction/ITransactionHandler.cs` | Replace `ListAsync` + `GetByIdAsync` with `GetAsync(ListTransactionsRequest)` (add `Id?` to existing filter) |
| `Application/Features/Transaction/TransactionHandler.cs` | Implement unified `GetAsync` |
| `Application/Features/Transaction/ListTransactionsRequest.cs` | Add `Id? Id` property |
| `Controllers/BankAccountController.cs` | Replace `List`+`GetById` with `Get(filter)`, remove `CreatedAtAction` ref to GetById |
| `Controllers/CategoryController.cs` | Same pattern |
| `Controllers/StoreController.cs` | Same pattern |
| `Controllers/FixedExpenseController.cs` | Same pattern |
| `Controllers/TransactionController.cs` | Same pattern |
| `Controllers/AgentApprovalController.cs` | Remove 3 `/// <summary>` comments |
| `Controllers/ReportController.cs` | Remove summary comment, switch to `GetMonthlyReportRequest`, remove inline validation |
| `Controllers/BankImportController.cs` | Remove multi-line summary comment |
| `Program.cs` | Add `AddFluentValidationAutoValidation()` + `AddValidatorsFromAssemblyContaining<Program>()` |
| `Infrastructure/Utils/Agents/Finance/ClassificationAgent.cs` | Update namespace import for moved `ClassificationResult` |
| `Infrastructure/Utils/Agents/Finance/SecurityValidationAgent.cs` | Update namespace import for moved `SecurityValidationResult` |
| `Infrastructure/Utils/Agents/Tools/FinancialAnalysisTool.cs` (or similar) | Update namespace import for moved `FinancialAnalysisResponse` |

### Files to Delete

| Path | Reason |
|---|---|
| `Application/Responses/ClassificationResult.cs` | Moved to `Features/BankImport/` |
| `Application/Responses/FinancialAnalysisResponse.cs` | Moved to `Features/Chat/` |
| `Application/Responses/SecurityValidationResult.cs` | Moved to `Features/AgentApproval/` |

---

## Task 1: Register FluentValidation in Program.cs

**Files:**
- Modify: `BlueBerryFinance.API/Program.cs`

- [ ] **Step 1: Add FluentValidation registrations**

In `Program.cs`, immediately after `builder.Services.AddControllers();`, add:

```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

Add the using at the top:

```csharp
using FluentValidation;
```

- [ ] **Step 2: Build to confirm it compiles**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```bash
git add BlueBerryFinance/BlueBerryFinance.API/Program.cs
git commit -m "feat: register FluentValidation auto-validation in DI"
```

---

## Task 2: Move Response Classes into Feature Folders

**Files:**
- Create: `BlueBerryFinance.API/Application/Features/AgentApproval/SecurityValidationResult.cs`
- Create: `BlueBerryFinance.API/Application/Features/Chat/FinancialAnalysisResponse.cs`
- Create: `BlueBerryFinance.API/Application/Features/BankImport/ClassificationResult.cs`
- Delete: `BlueBerryFinance.API/Application/Responses/SecurityValidationResult.cs`
- Delete: `BlueBerryFinance.API/Application/Responses/FinancialAnalysisResponse.cs`
- Delete: `BlueBerryFinance.API/Application/Responses/ClassificationResult.cs`

- [ ] **Step 1: Create `SecurityValidationResult.cs` in AgentApproval**

```csharp
// BlueBerryFinance.API/Application/Features/AgentApproval/SecurityValidationResult.cs
using System.ComponentModel;

namespace BlueBerryFinance.API.Application.Features.AgentApproval
{
    public class SecurityValidationResult
    {
        [Description("Whether the proposed operation is safe and valid for this user")]
        public bool IsValid { get; set; }

        [Description("Reason for the validation decision")]
        public string Reason { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 2: Create `FinancialAnalysisResponse.cs` in Chat**

```csharp
// BlueBerryFinance.API/Application/Features/Chat/FinancialAnalysisResponse.cs
using System.ComponentModel;

namespace BlueBerryFinance.API.Application.Features.Chat
{
    public class FinancialAnalysisResponse
    {
        [Description("The main answer to the user's question")]
        public string Answer { get; set; } = string.Empty;

        [Description("Insights derived from the financial data analysis")]
        public string Insights { get; set; } = string.Empty;

        [Description("Recommendations based on the financial analysis")]
        public string Recommendations { get; set; } = string.Empty;

        [Description("A summary of the financial data analyzed")]
        public string DataSummary { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 3: Create `ClassificationResult.cs` in BankImport**

```csharp
// BlueBerryFinance.API/Application/Features/BankImport/ClassificationResult.cs
using System.ComponentModel;

namespace BlueBerryFinance.API.Application.Features.BankImport
{
    public class ClassificationResult
    {
        [Description("Transaction type: Income or Expense")]
        public string Type { get; set; } = string.Empty;

        [Description("Origin type: Person, Company, or Store")]
        public string OriginType { get; set; } = string.Empty;

        [Description("Cleaned transaction description")]
        public string Description { get; set; } = string.Empty;

        [Description("Suggested category name (e.g. Food & Dining, Transport, Salary)")]
        public string CategorySuggestion { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 4: Update namespace imports in consuming agent files**

Find all files that import `BlueBerryFinance.API.Application.Responses` and update their usings. Run:

```bash
grep -rl "Application.Responses" BlueBerryFinance/BlueBerryFinance.API/
```

For each found file:
- Replace `using BlueBerryFinance.API.Application.Responses;` with the appropriate new namespace:
  - `ClassificationResult` consumers → `using BlueBerryFinance.API.Application.Features.BankImport;`
  - `FinancialAnalysisResponse` consumers → `using BlueBerryFinance.API.Application.Features.Chat;`
  - `SecurityValidationResult` consumers → `using BlueBerryFinance.API.Application.Features.AgentApproval;`

- [ ] **Step 5: Delete old response files**

```bash
Remove-Item "BlueBerryFinance/BlueBerryFinance.API/Application/Responses/ClassificationResult.cs"
Remove-Item "BlueBerryFinance/BlueBerryFinance.API/Application/Responses/FinancialAnalysisResponse.cs"
Remove-Item "BlueBerryFinance/BlueBerryFinance.API/Application/Responses/SecurityValidationResult.cs"
Remove-Item "BlueBerryFinance/BlueBerryFinance.API/Application/Responses/" -Recurse
```

- [ ] **Step 6: Build to confirm no broken references**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "refactor: move response classes into their respective feature folders"
```

---

## Task 3: Remove XML Summary Comments from Controllers

**Files:**
- Modify: `BlueBerryFinance.API/Controllers/AgentApprovalController.cs`
- Modify: `BlueBerryFinance.API/Controllers/ReportController.cs`
- Modify: `BlueBerryFinance.API/Controllers/BankImportController.cs`

- [ ] **Step 1: Remove all `/// <summary>` blocks from `AgentApprovalController.cs`**

Remove these three blocks (lines immediately above `GetPending`, `Approve`, and `Reject`):

```csharp
/// <summary>Returns all pending agent approvals for the current user.</summary>
```
```csharp
/// <summary>Approves a pending agent action and executes it immediately.</summary>
```
```csharp
/// <summary>Rejects a pending agent action without executing it.</summary>
```

- [ ] **Step 2: Remove summary comment from `ReportController.cs`**

Remove:

```csharp
/// <summary>GET /api/v1.0/report/monthly?year=2025&month=3</summary>
```

- [ ] **Step 3: Remove multi-line summary comment from `BankImportController.cs`**

Remove the entire block:

```csharp
/// <summary>
/// Upload a CSV bank statement export and import transactions into the given bank account.
/// Supported formats: ActivoBank, CGD, BPI, Millennium BCP (semicolon-separated Portuguese CSV).
/// Duplicate rows (same date + amount + description) are automatically skipped.
/// </summary>
```

- [ ] **Step 4: Build to confirm clean**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add BlueBerryFinance/BlueBerryFinance.API/Controllers/AgentApprovalController.cs
git add BlueBerryFinance/BlueBerryFinance.API/Controllers/ReportController.cs
git add BlueBerryFinance/BlueBerryFinance.API/Controllers/BankImportController.cs
git commit -m "refactor: remove XML summary comments from controllers"
```

---

## Task 4: BankAccount — Filter Request + Unified GET + Validator

**Files:**
- Create: `BlueBerryFinance.API/Application/Features/BankAccount/BankAccountFilterRequest.cs`
- Create: `BlueBerryFinance.API/Application/Features/BankAccount/RegisterBankAccountRequestValidator.cs`
- Modify: `BlueBerryFinance.API/Application/Features/BankAccount/IBankAccountHandler.cs`
- Modify: `BlueBerryFinance.API/Application/Features/BankAccount/BankAccountHandler.cs`
- Modify: `BlueBerryFinance.API/Controllers/BankAccountController.cs`

- [ ] **Step 1: Create `BankAccountFilterRequest.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/BankAccount/BankAccountFilterRequest.cs
namespace BlueBerryFinance.API.Application.Features.BankAccount
{
    public class BankAccountFilterRequest
    {
        public Guid? Id { get; set; }
    }
}
```

- [ ] **Step 2: Create `RegisterBankAccountRequestValidator.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/BankAccount/RegisterBankAccountRequestValidator.cs
using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.BankAccount
{
    public class RegisterBankAccountRequestValidator : AbstractValidator<RegisterBankAccountRequest>
    {
        public RegisterBankAccountRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.CurrencyId)
                .NotEmpty().WithMessage("CurrencyId is required.");

            RuleFor(x => x.InitialBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Initial balance cannot be negative.");
        }
    }
}
```

- [ ] **Step 3: Update `IBankAccountHandler.cs`**

Replace the interface body:

```csharp
// BlueBerryFinance.API/Application/Features/BankAccount/IBankAccountHandler.cs
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.BankAccount
{
    public interface IBankAccountHandler
    {
        Task<IReadOnlyList<BankAccountViewModel>> GetAsync(BankAccountFilterRequest filter, Guid userId, CancellationToken ct = default);
        Task<BankAccountViewModel> RegisterAsync(RegisterBankAccountRequest request, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
```

- [ ] **Step 4: Update `BankAccountHandler.cs`**

Replace the entire class body. Remove `ListAsync` and `GetByIdAsync`. Add `GetAsync`. Keep `RegisterAsync` and `DeleteAsync` unchanged.

The new `GetAsync` method:

```csharp
public async Task<IReadOnlyList<BankAccountViewModel>> GetAsync(
    BankAccountFilterRequest filter, Guid userId, CancellationToken ct = default)
{
    var query = _db.BankAccounts
        .AsNoTracking()
        .Where(b => b.UserId == userId);

    if (filter.Id.HasValue)
        query = query.Where(b => b.Id == filter.Id.Value);

    return await query
        .Select(b => new BankAccountViewModel
        {
            Id = b.Id,
            UserId = b.UserId,
            Name = b.Name,
            Bank = b.Bank.ToString(),
            Country = b.Country.ToString(),
            CurrencyCode = b.Currency.Code.ToString(),
            CurrencySymbol = b.Currency.Symbol,
            Balance = b.Balance,
            Active = b.Active == 1
        })
        .ToListAsync(ct);
}
```

The `RegisterAsync` internal call to `GetByIdAsync` must be replaced with a call to `GetAsync`:

```csharp
return (await GetAsync(new BankAccountFilterRequest { Id = entity.Id }, userId, ct)).First();
```

- [ ] **Step 5: Update `BankAccountController.cs`**

Replace the full controller with:

```csharp
using BlueBerryFinance.API.Application.Features.BankAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/bank-account")]
    [Authorize]
    public class BankAccountController : BaseController
    {
        private readonly IBankAccountHandler _handler;
        private readonly ILogger<BankAccountController> _logger;

        public BankAccountController(IBankAccountHandler handler, ILogger<BankAccountController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] BankAccountFilterRequest filter, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetAsync(filter, CurrentUserId, ct);

                if (filter.Id.HasValue && result.Count == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterBankAccountRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.RegisterAsync(request, CurrentUserId, ct);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id, CurrentUserId, ct);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
```

- [ ] **Step 6: Build**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: BankAccount — unified GET filter, FluentValidation validator"
```

---

## Task 5: Category — Filter Request + Unified GET + Validator

**Files:**
- Create: `BlueBerryFinance.API/Application/Features/Category/CategoryFilterRequest.cs`
- Create: `BlueBerryFinance.API/Application/Features/Category/RegisterCategoryRequestValidator.cs`
- Modify: `BlueBerryFinance.API/Application/Features/Category/ICategoryHandler.cs`
- Modify: `BlueBerryFinance.API/Application/Features/Category/CategoryHandler.cs`
- Modify: `BlueBerryFinance.API/Controllers/CategoryController.cs`

- [ ] **Step 1: Create `CategoryFilterRequest.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Category/CategoryFilterRequest.cs
namespace BlueBerryFinance.API.Application.Features.Category
{
    public class CategoryFilterRequest
    {
        public Guid? Id { get; set; }
    }
}
```

- [ ] **Step 2: Create `RegisterCategoryRequestValidator.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Category/RegisterCategoryRequestValidator.cs
using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Category
{
    public class RegisterCategoryRequestValidator : AbstractValidator<RegisterCategoryRequest>
    {
        public RegisterCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.Icon)
                .NotEmpty().WithMessage("Icon is required.");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color is required.");

            RuleFor(x => x.Type)
                .Must(t => t == "Income" || t == "Expense")
                .WithMessage("Type must be 'Income' or 'Expense'.");
        }
    }
}
```

- [ ] **Step 3: Update `ICategoryHandler.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Category/ICategoryHandler.cs
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Category
{
    public interface ICategoryHandler
    {
        Task<IReadOnlyList<CategoryViewModel>> GetAsync(CategoryFilterRequest filter, CancellationToken ct = default);
        Task<CategoryViewModel> RegisterAsync(RegisterCategoryRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
```

- [ ] **Step 4: Update `CategoryHandler.cs`**

Remove `ListAsync` and `GetByIdAsync`. Add:

```csharp
public async Task<IReadOnlyList<CategoryViewModel>> GetAsync(
    CategoryFilterRequest filter, CancellationToken ct = default)
{
    var query = _db.Categories.AsNoTracking();

    if (filter.Id.HasValue)
        query = query.Where(c => c.Id == filter.Id.Value);

    return await query
        .Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            Color = c.Color,
            Type = c.Type,
            Active = c.Active == 1
        })
        .ToListAsync(ct);
}
```

Update `RegisterAsync` internal call:

```csharp
return (await GetAsync(new CategoryFilterRequest { Id = entity.Id }, ct)).First();
```

- [ ] **Step 5: Update `CategoryController.cs`**

```csharp
using BlueBerryFinance.API.Application.Features.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/category")]
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly ICategoryHandler _handler;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryHandler handler, ILogger<CategoryController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] CategoryFilterRequest filter, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetAsync(filter, ct);

                if (filter.Id.HasValue && result.Count == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterCategoryRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.RegisterAsync(request, ct);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id, ct);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
```

- [ ] **Step 6: Build**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: Category — unified GET filter, FluentValidation validator"
```

---

## Task 6: Store — Filter Request + Unified GET + Validator

**Files:**
- Create: `BlueBerryFinance.API/Application/Features/Store/StoreFilterRequest.cs`
- Create: `BlueBerryFinance.API/Application/Features/Store/RegisterStoreRequestValidator.cs`
- Modify: `BlueBerryFinance.API/Application/Features/Store/IStoreHandler.cs`
- Modify: `BlueBerryFinance.API/Application/Features/Store/StoreHandler.cs`
- Modify: `BlueBerryFinance.API/Controllers/StoreController.cs`

- [ ] **Step 1: Create `StoreFilterRequest.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Store/StoreFilterRequest.cs
namespace BlueBerryFinance.API.Application.Features.Store
{
    public class StoreFilterRequest
    {
        public Guid? Id { get; set; }
    }
}
```

- [ ] **Step 2: Create `RegisterStoreRequestValidator.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Store/RegisterStoreRequestValidator.cs
using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Store
{
    public class RegisterStoreRequestValidator : AbstractValidator<RegisterStoreRequest>
    {
        public RegisterStoreRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");
        }
    }
}
```

- [ ] **Step 3: Update `IStoreHandler.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Store/IStoreHandler.cs
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Store
{
    public interface IStoreHandler
    {
        Task<IReadOnlyList<StoreViewModel>> GetAsync(StoreFilterRequest filter, CancellationToken ct = default);
        Task<StoreViewModel> RegisterAsync(RegisterStoreRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
```

- [ ] **Step 4: Update `StoreHandler.cs`**

Remove `ListAsync` and `GetByIdAsync`. Add:

```csharp
public async Task<IReadOnlyList<StoreViewModel>> GetAsync(
    StoreFilterRequest filter, CancellationToken ct = default)
{
    var query = _db.Stores.AsNoTracking();

    if (filter.Id.HasValue)
        query = query.Where(s => s.Id == filter.Id.Value);

    return await query
        .Select(s => new StoreViewModel
        {
            Id = s.Id,
            Name = s.Name,
            CategoryId = s.CategoryId,
            CategoryName = s.Category.Name,
            Active = s.Active == 1
        })
        .ToListAsync(ct);
}
```

Update `RegisterAsync` internal call:

```csharp
return (await GetAsync(new StoreFilterRequest { Id = entity.Id }, ct)).First();
```

- [ ] **Step 5: Update `StoreController.cs`**

```csharp
using BlueBerryFinance.API.Application.Features.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/store")]
    [Authorize]
    public class StoreController : BaseController
    {
        private readonly IStoreHandler _handler;
        private readonly ILogger<StoreController> _logger;

        public StoreController(IStoreHandler handler, ILogger<StoreController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] StoreFilterRequest filter, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetAsync(filter, ct);

                if (filter.Id.HasValue && result.Count == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody] RegisterStoreRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.RegisterAsync(request, ct);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id, ct);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
```

- [ ] **Step 6: Build**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: Store — unified GET filter, FluentValidation validator"
```

---

## Task 7: FixedExpense — Filter Request + Unified GET + Validator

**Files:**
- Create: `BlueBerryFinance.API/Application/Features/FixedExpense/FixedExpenseFilterRequest.cs`
- Create: `BlueBerryFinance.API/Application/Features/FixedExpense/RegisterFixedExpenseRequestValidator.cs`
- Modify: `BlueBerryFinance.API/Application/Features/FixedExpense/IFixedExpenseHandler.cs`
- Modify: `BlueBerryFinance.API/Application/Features/FixedExpense/FixedExpenseHandler.cs`
- Modify: `BlueBerryFinance.API/Controllers/FixedExpenseController.cs`

- [ ] **Step 1: Create `FixedExpenseFilterRequest.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/FixedExpense/FixedExpenseFilterRequest.cs
namespace BlueBerryFinance.API.Application.Features.FixedExpense
{
    public class FixedExpenseFilterRequest
    {
        public Guid? Id { get; set; }
    }
}
```

- [ ] **Step 2: Create `RegisterFixedExpenseRequestValidator.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/FixedExpense/RegisterFixedExpenseRequestValidator.cs
using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.FixedExpense
{
    public class RegisterFixedExpenseRequestValidator : AbstractValidator<RegisterFixedExpenseRequest>
    {
        public RegisterFixedExpenseRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.CurrencyId)
                .NotEmpty().WithMessage("CurrencyId is required.");

            RuleFor(x => x.StoreId)
                .NotEmpty().WithMessage("StoreId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.DayOfMonth)
                .InclusiveBetween(1, 31).WithMessage("DayOfMonth must be between 1 and 31.");
        }
    }
}
```

- [ ] **Step 3: Update `IFixedExpenseHandler.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/FixedExpense/IFixedExpenseHandler.cs
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.FixedExpense
{
    public interface IFixedExpenseHandler
    {
        Task<IReadOnlyList<FixedExpenseViewModel>> GetAsync(FixedExpenseFilterRequest filter, Guid userId, CancellationToken ct = default);
        Task<FixedExpenseViewModel> RegisterAsync(RegisterFixedExpenseRequest request, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
```

- [ ] **Step 4: Update `FixedExpenseHandler.cs`**

Remove `ListAsync` and `GetByIdAsync`. Add:

```csharp
public async Task<IReadOnlyList<FixedExpenseViewModel>> GetAsync(
    FixedExpenseFilterRequest filter, Guid userId, CancellationToken ct = default)
{
    var query = _db.FixedExpenses
        .AsNoTracking()
        .Where(f => f.UserId == userId);

    if (filter.Id.HasValue)
        query = query.Where(f => f.Id == filter.Id.Value);

    return await query
        .Select(f => new FixedExpenseViewModel
        {
            Id = f.Id,
            UserId = f.UserId,
            Name = f.Name,
            Description = f.Description,
            Amount = f.Amount,
            CurrencyCode = f.Currency.Code.ToString(),
            CurrencySymbol = f.Currency.Symbol,
            DayOfMonth = f.DayOfMonth,
            IsRecurring = f.IsRecurring,
            StoreName = f.Store.Name,
            Active = f.Active == 1
        })
        .ToListAsync(ct);
}
```

Update `RegisterAsync` internal call:

```csharp
return (await GetAsync(new FixedExpenseFilterRequest { Id = entity.Id }, userId, ct)).First();
```

- [ ] **Step 5: Update `FixedExpenseController.cs`**

```csharp
using BlueBerryFinance.API.Application.Features.FixedExpense;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/fixed-expense")]
    [Authorize]
    public class FixedExpenseController : BaseController
    {
        private readonly IFixedExpenseHandler _handler;
        private readonly ILogger<FixedExpenseController> _logger;

        public FixedExpenseController(IFixedExpenseHandler handler, ILogger<FixedExpenseController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] FixedExpenseFilterRequest filter, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetAsync(filter, CurrentUserId, ct);

                if (filter.Id.HasValue && result.Count == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody] RegisterFixedExpenseRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.RegisterAsync(request, CurrentUserId, ct);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id, CurrentUserId, ct);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
```

- [ ] **Step 6: Build**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: FixedExpense — unified GET filter, FluentValidation validator"
```

---

## Task 8: Transaction — Add Id Filter + Unified GET + Validators

**Files:**
- Modify: `BlueBerryFinance.API/Application/Features/Transaction/ListTransactionsRequest.cs`
- Create: `BlueBerryFinance.API/Application/Features/Transaction/RegisterTransactionRequestValidator.cs`
- Create: `BlueBerryFinance.API/Application/Features/Transaction/UpdateTransactionRequestValidator.cs`
- Modify: `BlueBerryFinance.API/Application/Features/Transaction/ITransactionHandler.cs`
- Modify: `BlueBerryFinance.API/Application/Features/Transaction/TransactionHandler.cs`
- Modify: `BlueBerryFinance.API/Controllers/TransactionController.cs`

- [ ] **Step 1: Add `Id?` to `ListTransactionsRequest.cs`**

Add the property at the top of the class:

```csharp
public Guid? Id { get; set; }
```

Full updated class:

```csharp
// BlueBerryFinance.API/Application/Features/Transaction/ListTransactionsRequest.cs
namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public class ListTransactionsRequest
    {
        public Guid? Id { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? BankAccountId { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; }
    }
}
```

- [ ] **Step 2: Create `RegisterTransactionRequestValidator.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Transaction/RegisterTransactionRequestValidator.cs
using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public class RegisterTransactionRequestValidator : AbstractValidator<RegisterTransactionRequest>
    {
        public RegisterTransactionRequestValidator()
        {
            RuleFor(x => x.BankAccountId)
                .NotEmpty().WithMessage("BankAccountId is required.");

            RuleFor(x => x.StoreId)
                .NotEmpty().WithMessage("StoreId is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            RuleFor(x => x.CurrencyId)
                .NotEmpty().WithMessage("CurrencyId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");
        }
    }
}
```

- [ ] **Step 3: Create `UpdateTransactionRequestValidator.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Transaction/UpdateTransactionRequestValidator.cs
using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
    {
        public UpdateTransactionRequestValidator()
        {
            RuleFor(x => x.StoreId)
                .NotEmpty().WithMessage("StoreId is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");
        }
    }
}
```

- [ ] **Step 4: Update `ITransactionHandler.cs`**

Replace `ListAsync` and `GetByIdAsync` with a single `GetAsync`:

```csharp
// BlueBerryFinance.API/Application/Features/Transaction/ITransactionHandler.cs
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public interface ITransactionHandler
    {
        Task<PagedResult<TransactionViewModel>> GetAsync(ListTransactionsRequest request, Guid userId, CancellationToken ct = default);
        Task<TransactionViewModel> RegisterAsync(RegisterTransactionRequest request, Guid userId, CancellationToken ct = default);
        Task<TransactionViewModel?> UpdateAsync(UpdateTransactionRequest request, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
```

- [ ] **Step 5: Update `TransactionHandler.cs`**

Rename `ListAsync` → `GetAsync`. Add `Id` filter at the top of the query builder, before the other filters:

```csharp
public async Task<PagedResult<TransactionViewModel>> GetAsync(
    ListTransactionsRequest request, Guid userId, CancellationToken ct = default)
{
    var query = _db.Transactions
        .AsNoTracking()
        .Where(t => t.UserId == userId);

    if (request.Id.HasValue)
        query = query.Where(t => t.Id == request.Id.Value);

    if (request.BankAccountId.HasValue)
        query = query.Where(t => t.BankAccountId == request.BankAccountId.Value);

    if (request.CategoryId.HasValue)
        query = query.Where(t => t.CategoryId == request.CategoryId.Value);

    if (request.DateFrom.HasValue)
        query = query.Where(t => t.TransactionDate >= request.DateFrom.Value);

    if (request.DateTo.HasValue)
        query = query.Where(t => t.TransactionDate <= request.DateTo.Value);

    if (!string.IsNullOrWhiteSpace(request.Search))
        query = query.Where(t => t.Description.Contains(request.Search) || t.Store.Name.Contains(request.Search));

    var total = await query.CountAsync(ct);
    var items = await query
        .OrderByDescending(t => t.TransactionDate)
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(t => new TransactionViewModel
        {
            Id = t.Id,
            UserId = t.UserId,
            BankAccountId = t.BankAccountId,
            BankAccountName = t.BankAccount.Name,
            StoreId = t.StoreId,
            StoreName = t.Store.Name,
            CategoryId = t.CategoryId,
            CategoryName = t.Category.Name,
            CategoryColor = t.Category.Color,
            CurrencyCode = t.Currency.Code.ToString(),
            CurrencySymbol = t.Currency.Symbol,
            TransactionType = t.TransactionType.ToString(),
            Source = t.Source.ToString(),
            Amount = t.Amount,
            Description = t.Description,
            TransactionDate = t.TransactionDate,
            ImageUrl = t.ImageUrl,
            CorrelationId = t.CorrelationId,
            InsertionDate = t.InsertionDate
        })
        .ToListAsync(ct);

    return new PagedResult<TransactionViewModel>
    {
        Items = items,
        TotalCount = total,
        Page = request.Page,
        PageSize = request.PageSize
    };
}
```

Remove the old `GetByIdAsync` method entirely. Update `RegisterAsync` internal call to:

```csharp
return (await GetAsync(new ListTransactionsRequest { Id = entity.Id }, userId, ct)).Items.First();
```

Update `UpdateAsync` internal call to:

```csharp
return (await GetAsync(new ListTransactionsRequest { Id = entity.Id }, userId, ct)).Items.FirstOrDefault();
```

- [ ] **Step 6: Update `TransactionController.cs`**

Replace the controller with:

```csharp
using BlueBerryFinance.API.Application.Features.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Route("api/v1.0/transaction")]
    [Authorize]
    public class TransactionController : BaseController
    {
        private readonly ITransactionHandler _handler;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionHandler handler, ILogger<TransactionController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] ListTransactionsRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetAsync(request, CurrentUserId, ct);

                if (request.Id.HasValue && result.TotalCount == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterTransactionRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.RegisterAsync(request, CurrentUserId, ct);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request, CancellationToken ct)
        {
            try
            {
                request.Id = id;
                var result = await _handler.UpdateAsync(request, CurrentUserId, ct);
                return result is null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await _handler.DeleteAsync(id, CurrentUserId, ct);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
```

- [ ] **Step 7: Build**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "feat: Transaction — unified GET filter, FluentValidation validators"
```

---

## Task 9: Report — GetMonthlyReportRequest + Validator

**Files:**
- Create: `BlueBerryFinance.API/Application/Features/Report/GetMonthlyReportRequest.cs`
- Create: `BlueBerryFinance.API/Application/Features/Report/GetMonthlyReportRequestValidator.cs`
- Modify: `BlueBerryFinance.API/Controllers/ReportController.cs`

- [ ] **Step 1: Create `GetMonthlyReportRequest.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Report/GetMonthlyReportRequest.cs
namespace BlueBerryFinance.API.Application.Features.Report
{
    public class GetMonthlyReportRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
```

- [ ] **Step 2: Create `GetMonthlyReportRequestValidator.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Report/GetMonthlyReportRequestValidator.cs
using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Report
{
    public class GetMonthlyReportRequestValidator : AbstractValidator<GetMonthlyReportRequest>
    {
        public GetMonthlyReportRequestValidator()
        {
            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.");

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
        }
    }
}
```

- [ ] **Step 3: Update `ReportController.cs`**

Replace the controller (summary comment was already removed in Task 3). Replace inline `year`/`month` params and manual validation with the new request object:

```csharp
using BlueBerryFinance.API.Application.Features.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueBerryFinance.API.Controllers
{
    [Authorize(Policy = "UserOrAdmin")]
    public class ReportController : BaseController
    {
        private readonly IReportHandler _handler;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportHandler handler, ILogger<ReportController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet("report/monthly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMonthly([FromQuery] GetMonthlyReportRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _handler.GetMonthlyAsync(request.Year, request.Month, CurrentUserId, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, _logger);
            }
        }
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: Report — replace inline year/month params with GetMonthlyReportRequest + FluentValidation"
```

---

## Task 10: Chat — ChatStreamRequest Validator

**Files:**
- Create: `BlueBerryFinance.API/Application/Features/Chat/ChatStreamRequestValidator.cs`

- [ ] **Step 1: Create `ChatStreamRequestValidator.cs`**

```csharp
// BlueBerryFinance.API/Application/Features/Chat/ChatStreamRequestValidator.cs
using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Chat
{
    public class ChatStreamRequestValidator : AbstractValidator<ChatStreamRequest>
    {
        public ChatStreamRequestValidator()
        {
            RuleFor(x => x.Prompt)
                .NotEmpty().WithMessage("Prompt is required.");
        }
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.API/BlueBerryFinance.API.csproj
```

Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add BlueBerryFinance/BlueBerryFinance.API/Application/Features/Chat/ChatStreamRequestValidator.cs
git commit -m "feat: Chat — add FluentValidation validator for ChatStreamRequest"
```

---

## Task 11: Final Build Verification

- [ ] **Step 1: Full solution build**

```bash
dotnet build BlueBerryFinance/BlueBerryFinance.slnx
```

Expected: All projects build successfully, 0 errors.

- [ ] **Step 2: Run existing tests**

```bash
dotnet test BlueBerryFinance/BlueBerryFinance.Tests/BlueBerryFinance.Tests.csproj --no-build
```

Expected: All tests pass.

- [ ] **Step 3: Final commit if any fixups were needed**

```bash
git add -A
git commit -m "fix: resolve any remaining compilation issues after refactor"
```
