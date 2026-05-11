# Design Spec: Repository Layer + Pure Moq Tests

**Date:** 2026-05-11  
**Status:** Approved  
**Scope:** `BlueBerryFinance.API` + `BlueBerryFinance.Tests`

---

## Goal

Introduce a repository abstraction layer between the application handlers and EF Core, enabling handler tests to be written as pure unit tests using Moq — with no in-memory database involvement.

---

## Problem Statement

The current handler tests (`AuthHandlerTests`, `CategoryHandlerTests`, `TransactionHandlerBalanceTests`) depend on `TestServiceFactory`, which wires a full in-memory EF Core `AppDbContext`. This means:

- Each test is an integration test rather than a unit test
- Setup is verbose and duplicated across test methods
- Tests are coupled to EF behavior (global query filters, entity tracking, etc.)
- There is no mockable seam between the handler and the database

Handlers currently depend directly on `AppDbContext`, making Moq-based testing impractical without introducing an abstraction.

---

## Solution Overview

Introduce repository interfaces and concrete EF Core implementations for the three entities that have existing handler tests: `Category`, `Transaction`, and `Auth` (User). Refactor the corresponding handlers to depend on these interfaces. Rewrite the three test classes to use static Moq mock classes — following the pattern where each mock class owns its own seed data and exposes a `GetMock()` factory method.

---

## Architecture

### New Layer: Repositories

```
BlueBerryFinance.API/
└── Data/
    └── Repositories/
        ├── Interfaces/
        │   ├── ICategoryRepository.cs
        │   ├── ITransactionRepository.cs
        │   └── IAuthRepository.cs
        ├── CategoryRepository.cs
        ├── TransactionRepository.cs
        └── AuthRepository.cs
```

Each repository wraps `AppDbContext` and exposes only the operations the handler needs. There is no shared `IUnitOfWork` — `SaveAsync()` lives on each repository interface to keep the abstraction flat and explicit.

### Handler Changes

| Handler | Before | After |
|---|---|---|
| `CategoryHandler` | `CategoryHandler(AppDbContext db)` | `CategoryHandler(ICategoryRepository repo)` |
| `TransactionHandler` | `TransactionHandler(AppDbContext db)` | `TransactionHandler(ITransactionRepository repo)` |
| `AuthHandler` | `AuthHandler(AppDbContext db, IJwtService jwt, IOptions<JwtOptions> opts)` | `AuthHandler(IAuthRepository repo, IJwtService jwt, IOptions<JwtOptions> opts)` |

Other handlers (`BankAccountHandler`, `StoreHandler`, `FixedExpenseHandler`, `ReportHandler`) are not changed — no tests exist for them and they are out of scope.

### DI Registration

`Program.cs` registers the new repositories:

```csharp
services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<ITransactionRepository, TransactionRepository>();
services.AddScoped<IAuthRepository, AuthRepository>();
```

---

## Repository Interfaces

### `ICategoryRepository`

```csharp
public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task AddAsync(Category category);
    Task DeleteAsync(Category category);
    Task SaveAsync();
}
```

### `ITransactionRepository`

```csharp
public interface ITransactionRepository
{
    Task<(List<Transaction> Items, int TotalCount)> GetPagedAsync(
        Guid userId, int page, int pageSize,
        Guid? bankAccountId, Guid? categoryId,
        DateTime? dateFrom, DateTime? dateTo, string? search);
    Task<Transaction?> GetByIdAsync(Guid id, Guid userId);
    Task<BankAccount?> GetBankAccountAsync(Guid id, Guid userId);
    Task AddAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);
    Task SaveAsync();
}
```

### `IAuthRepository`

```csharp
public interface IAuthRepository
{
    Task<User?> GetByEmailAsync(string email);
}
```

---

## Test Structure

### New Mock Classes

```
BlueBerryFinance.Tests/
└── Data/
    └── Mocks/
        ├── MockFinancialAssistantAgent.cs   (unchanged)
        ├── MockAuthRepository.cs            (new)
        ├── MockCategoryRepository.cs        (new)
        └── MockTransactionRepository.cs     (new)
```

Each mock class follows this contract:

```csharp
public class MockCategoryRepository
{
    public static Mock<ICategoryRepository> GetMock()
    {
        var mock = new Mock<ICategoryRepository>();

        // Setup methods with realistic behavior using CategoriesSeed()
        mock.Setup(r => r.GetAllAsync()).ReturnsAsync(CategoriesSeed());
        mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => CategoriesSeed().FirstOrDefault(c => c.Id == id));
        mock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.DeleteAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
        mock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        return mock;
    }

    private static List<Category> CategoriesSeed() => new()
    {
        new Category { Id = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001"),
                       Name = "Food", Icon = "restaurant", Color = "#FF6B35", Type = "Expense", Active = 1 }
    };
}
```

### Refactored Test Classes

Each test class uses constructor injection:

```csharp
public class CategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository;

    public CategoryHandlerTests()
    {
        _categoryRepository = MockCategoryRepository.GetMock();
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsViewModel()
    {
        var handler = new CategoryHandler(_categoryRepository.Object);

        var result = await handler.RegisterAsync(new RegisterCategoryRequest
        {
            Name = "Food", Icon = "restaurant", Color = "#FF6B35", Type = "Expense"
        });

        result.Should().NotBeNull();
        result.Name.Should().Be("Food");
        _categoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _categoryRepository.Verify(r => r.SaveAsync(), Times.Once);
    }
}
```

Tests verify both the return value and that the correct repository methods were called with correct arguments.

---

## Trade-offs

| Concern | Decision |
|---|---|
| EF global query filter behavior (soft-delete) | Not covered by Moq tests. Filters are applied in concrete repository implementations. Integration tests can cover this later if needed. |
| Balance arithmetic on persisted entities | `TransactionHandler` balance logic is tested by verifying `SaveAsync()` is called with the correct entity state — not by reading back from DB. |
| `TestServiceFactory` | Kept unchanged. Available for future integration tests or controller-level WebApplicationFactory tests. |
| Handlers not covered (`BankAccount`, `Store`, etc.) | Out of scope. Will be addressed when tests for those handlers are added. |

---

## Out of Scope

- Controller-level tests
- `IUnitOfWork` pattern
- AutoMapper introduction
- `BankAccountHandler`, `StoreHandler`, `FixedExpenseHandler`, `ReportHandler`
- Any API endpoint changes

---

## Acceptance Criteria

1. `ICategoryRepository`, `ITransactionRepository`, `IAuthRepository` interfaces exist and are registered in DI
2. `CategoryHandler`, `TransactionHandler`, `AuthHandler` no longer depend on `AppDbContext` directly
3. `MockCategoryRepository`, `MockTransactionRepository`, `MockAuthRepository` static classes exist in `Tests/Data/Mocks/`
4. All 11 existing handler tests pass with the new Moq-based setup (no in-memory EF)
5. The project builds with no errors (`dotnet build`)
6. All tests pass (`dotnet test`)
