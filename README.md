[![Build and Push NuGet Package](https://github.com/vpetkovic/OperationResult/actions/workflows/build-nuget-push.yml/badge.svg?branch=develop)](https://github.com/vpetkovic/OperationResult/actions/workflows/build-nuget-push.yml)

| Package | NuGet | Version | Stats |
| --------------- | --------------- | --------------- | --------------- |
| `OperationResult.Core` | [`Install-Package OperationResult.Core`](https://www.nuget.org/packages/OperationResult.Core/) | ![Nuget](https://img.shields.io/nuget/v/OperationResult.Core) | ![Nuget](https://img.shields.io/nuget/dt/OperationResult.Core?label=%20Downloads)

# OperationResult

Provides consistent, strongly-typed generic return objects throughout different layers of an application, from database queries in repositories to service-level operations.

## Why

Every codebase I've touched has the same problem: one service method throws on failure, another returns null, a third returns a boolean, and a fourth returns a tuple with an error string. Callers never know what to expect. You end up writing defensive code everywhere, and error context gets lost between layers.

OperationResult replaces all of that with a single pattern. Every operation returns a result that tells you: did it succeed, what's the result, and if it failed, why. It carries errors and messages through your entire stack so nothing gets swallowed silently.

## Usage

### Basic Success and Failure

```csharp
// No result, just success/failure
OperationResult result = OperationResult.Ok();
OperationResult failure = OperationResult.Fail("Something went wrong");

if (result.IsSuccess)
    Console.WriteLine("It worked");

if (failure.IsFailure)
    Console.WriteLine(failure.ErrorMessage); // "Something went wrong"

// With a result value
OperationResult<User> userResult = OperationResult<User>.Ok(new User("John"));

if (userResult.IsSuccess)
    Console.WriteLine(userResult.Result!.Name); // "John"

// Failure with message only — message is always the first parameter
OperationResult<User> notFound = OperationResult<User>.Fail("User not found");

// Failure with message and untyped errors
OperationResult<User> failedResult = OperationResult<User>.Fail(
    errorMessage: "User creation failed",
    errors: new { Field = "Email", Reason = "Already taken" }
);

// Failure with strongly-typed errors
OperationResult<User, List<ValidationError>> typedFailure =
    OperationResult<User, List<ValidationError>>.Fail(
        errorMessage: "Validation failed",
        errors: new List<ValidationError>
        {
            new("Email", "Already taken"),
            new("Username", "Too short")
        }
    );
```

### Repository Method with TryAsync

Wraps your operation in a try-catch and returns a clean result. No more scattered exception handling.

```csharp
public async Task<OperationResult<User>> CreateUserAsync(string name, string email)
{
    // Simple overload — just return the result
    return await OperationResult<User>.TryAsync(async () =>
    {
        var user = new User(name, email);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    },
    exceptionHandler: ex => _logger.LogError(ex, "Failed to create user"));
}
```

When you need to return errors from within the operation (not just exceptions), use the tuple overload:

```csharp
public async Task<OperationResult<User>> CreateUserAsync(string name, string email)
{
    // Tuple overload — return (result, errors)
    return await OperationResult<User>.TryAsync(async () =>
    {
        var existing = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing != null)
            return (default(User)!, new { Reason = "Email already taken" }); // triggers failure

        var user = new User(name, email);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return (user, default); // success
    });
}
```

### Service Method with Strongly-Typed Errors

When you need callers to handle specific error types, not just strings.

```csharp
public record ValidationError(string Field, string Reason);

public async Task<OperationResult<Order, List<ValidationError>>> PlaceOrderAsync(OrderRequest request)
{
    return await OperationResult<Order, List<ValidationError>>.TryAsync(async () =>
    {
        var errors = new List<ValidationError>();

        if (request.Quantity <= 0)
            errors.Add(new("Quantity", "Must be greater than zero"));

        if (!await _inventory.IsInStockAsync(request.ProductId))
            errors.Add(new("ProductId", "Out of stock"));

        if (errors.Count > 0)
            return (default, errors); // triggers failure

        var order = await _orderRepository.CreateAsync(request);
        return (order, default); // success
    });
}
```

### Converting Entities with From / ToOperationResult

Turn any object into an OperationResult. Null values automatically become failures.

```csharp
// Static method
User? user = await _dbContext.Users.FindAsync(userId);
OperationResult<User> result = OperationResult<User>.From(user);
// user is null → IsFailure = true, ErrorMessage = "Entity is null"
// user exists → IsSuccess = true, Result = user

// With custom error message
OperationResult<User> result = OperationResult<User>.From(user, "User not found");

// Extension method — same behavior
OperationResult<User> result = user.ToOperationResult();
OperationResult<User> result = user.ToOperationResult("User not found");

// With strongly-typed errors
OperationResult<User, List<string>> result = OperationResult<User, List<string>>.From(
    entity: user,
    errors: new List<string> { "User not found" },
    errorMessage: "Lookup failed"
);
```

### Mapping Results to DTOs with ToDtoResponse

Map success and failure to a response type in one call.

```csharp
// Untyped errors
var result = await OperationResult<User>.TryAsync(async () => { ... });

var response = result.ToDtoResponse(
    successMapping: user => new UserResponse { Id = user.Id, Name = user.Name },
    failureMapping: error => new UserResponse { Error = error }
);

// Typed errors
var result = await OperationResult<Order, List<ValidationError>>.TryAsync(async () => { ... });

var response = result.ToDtoResponse(
    successMapping: order => new OrderResponse { OrderId = order.Id },
    failureMapping: (errors, message) => new OrderResponse
    {
        Error = message,
        ValidationErrors = errors
    }
);
```

### Exception Handling with Custom Message Provider

Override the default exception message with something meaningful to your callers.

```csharp
public async Task<OperationResult<Report>> GenerateReportAsync(Guid reportId)
{
    return await OperationResult<Report>.TryAsync(
        async () =>
        {
            var data = await _dataService.FetchAsync(reportId);
            return _reportBuilder.Build(data);
        },
        exceptionHandler: ex => _logger.LogError(ex, "Report generation failed"),
        customMessageProvider: ex => $"Could not generate report {reportId}: {ex.Message}"
    );
}
// On exception: IsFailure = true, ErrorMessage = "Could not generate report abc123: timeout"
```

### Sync Operations

Same pattern, no async. Works for in-memory logic, calculations, parsing.

```csharp
// Simple — just return the result
OperationResult<int> parsed = OperationResult<int>.Try(() => int.Parse(input));

// Tuple — return result + errors
OperationResult<Config, List<string>> config =
    OperationResult<Config, List<string>>.Try(() =>
    {
        var cfg = ConfigParser.Parse(filePath);
        return (cfg, default);
    });

// Void — just success/failure, no result
OperationResult result = OperationResult.Try(() =>
{
    _cache.Invalidate(key);
});
```

### OperationException for Domain Boundary Crossing

When domain code can't return an `OperationResult` (e.g., entity methods, value objects), throw an `OperationException` with structured errors. `TryAsync`/`Try` at the service boundary catches it and preserves the error context in the result.

```csharp
// Domain layer — throws structured exception
public class Order
{
    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new OperationException<string>("Invalid quantity", "Must be greater than zero");
    }
}

// Service boundary — TryAsync catches it, errors flow into the result
public async Task<OperationResult<Order>> CreateOrderAsync(Product product, int quantity)
{
    return await OperationResult<Order>.TryAsync(async () =>
    {
        var order = new Order();
        order.AddItem(product, quantity);  // may throw OperationException
        await _repo.SaveAsync(order);
        return order;
    });
}
// If AddItem throws: IsFailure = true, ErrorMessage = "Invalid quantity"

// Catch and inspect directly when needed
try
{
    order.AddItem(product, -1);
}
catch (OperationException<string> ex)
{
    string? error = ex.Errors;   // "Must be greater than zero"
    string message = ex.Message; // "Invalid quantity"
}
```

### Passing Results Between Layers

OperationResult flows naturally through your architecture. Each layer adds its own context without losing what came before.

**Endpoint (API layer):**

```csharp
app.MapPost("/users", async (CreateUserRequest req, UserService service) =>
{
    var result = await service.CreateUserAsync(req.Name, req.Email);

    return result.IsSuccess
        ? Results.Created($"/users/{result.Result!.Id}", result.Result)
        : Results.BadRequest(new { result.ErrorMessage, result.Errors });
});
```

**Service layer:**

```csharp
public class UserService
{
    private readonly UserRepository _repo;
    private readonly IEmailService _email;

    public async Task<OperationResult<User>> CreateUserAsync(string name, string email)
    {
        var result = await _repo.CreateUserAsync(name, email);

        if (result.IsFailure)
            return result; // pass failure straight through

        await _email.SendWelcomeAsync(result.Result!);
        return result;
    }
}
```

**Repository (data layer):**

```csharp
public class UserRepository
{
    private readonly AppDbContext _db;

    public async Task<OperationResult<User>> CreateUserAsync(string name, string email)
    {
        return await OperationResult<User>.TryAsync(async () =>
        {
            var user = new User(name, email);
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return user;
        },
        customMessageProvider: ex => $"Failed to persist user: {ex.Message}");
    }
}
```

The endpoint doesn't know how the user was created. The service doesn't know what database is behind the repository. Each layer returns `OperationResult<User>`, and failures bubble up with full context.

## Migrating from 0.x to 2.0

Version 2.0 is a full API redesign. Here's what changed:

### Breaking Changes

| 0.x | 2.0 | Notes |
|-----|-----|-------|
| `.Success` (bool) | `.IsSuccess` / `.IsFailure` | Property renamed; `IsFailure` added as convenience |
| `OperationResult<T>.IsSuccess(result)` | `OperationResult<T>.Ok(result)` | Factory renamed |
| `OperationResult<T>.IsFailure(errors, message)` | `OperationResult<T>.Fail(message, errors)` | Factory renamed, **parameter order swapped** — message first |
| `OperationResultExtensions.TryOperationAsync<T>(...)` | `OperationResult<T>.TryAsync(...)` | Now a static method on the type |
| `OperationResultExtensions.TryOperation<T>(...)` | `OperationResult<T>.Try(...)` | Now a static method on the type |
| `OperationExceptionExtensions.ThrowIfNullOrEmpty(errors)` | Removed | Use `Fail()` or tuple return instead of throwing |
| `OperationExceptionExtensions.Throw()` | Removed | Use `throw new OperationException(...)` directly when needed |
| `entity.ToOperationResult<T, TErrors>()` | `OperationResult<T, TErrors>.From(entity)` | Static method; untyped extension `.ToOperationResult()` still works |
| Target: `net6.0` | Target: `netstandard2.0` / `netstandard2.1` | Broader compatibility |

### New in 2.0

- **Simple `Try`/`TryAsync` overloads** — return just the result, no tuple ceremony for the happy path
- **`ToDtoResponse`** — map success/failure to a DTO in one call
- **`OperationResultBase<TSelf, TResult, TErrors>`** — shared base class eliminates duplication
- **`OperationBaseException<TErrors>`** — shared exception base class
- **`IsFailure` property** — `if (result.IsFailure)` reads naturally
- **`From` static method** — `OperationResult<T>.From(entity)` for entity conversion

### Quick Find-and-Replace

```
.Success           →  .IsSuccess
IsSuccess(         →  Ok(
IsFailure(errors,  →  Fail(errorMessage:, errors:   ← check parameter order
TryOperationAsync  →  TryAsync
TryOperation       →  Try
```

## Requirements

- .NET Standard 2.0+ (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)

## License

MIT
