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
OperationResult result = OperationResult.IsSuccess();
OperationResult failure = OperationResult.IsFailure(message: "Something went wrong");

if (result.Succeeded)
    Console.WriteLine(result.Message); // null — no message on success unless you set one

// With a result value
OperationResult<User> userResult = OperationResult<User>.IsSuccess(new User("John"));

if (userResult.Succeeded)
    Console.WriteLine(userResult.Result!.Name); // "John"

// Failure with untyped errors
OperationResult<User> failedResult = OperationResult<User>.IsFailure(
    errors: new { Field = "Email", Reason = "Already taken" },
    message: "User creation failed"
);

// Failure with strongly-typed errors
OperationResult<User, List<ValidationError>> typedFailure =
    OperationResult<User, List<ValidationError>>.IsFailure(
        errors: new List<ValidationError>
        {
            new("Email", "Already taken"),
            new("Username", "Too short")
        },
        message: "Validation failed"
    );
```

### Repository Method with TryOperationAsync

Wraps your operation in a try-catch and returns a clean result. No more scattered exception handling.

```csharp
public async Task<OperationResult<User>> CreateUserAsync(string name, string email)
{
    return await OperationResultExtensions.TryOperationAsync<User>(async () =>
    {
        var user = new User(name, email);

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return (user, default); // (result, errors) — errors is null on success
    },
    onException: ex => _logger.LogError(ex, "Failed to create user"));
}
```

### Service Method with Strongly-Typed Errors

When you need callers to handle specific error types, not just strings.

```csharp
public record ValidationError(string Field, string Reason);

public async Task<OperationResult<Order, List<ValidationError>>> PlaceOrderAsync(OrderRequest request)
{
    return await OperationResultExtensions.TryOperationAsync<Order, List<ValidationError>>(async () =>
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

### Converting Entities with ToOperationResult

Turn any object into an OperationResult. Null values automatically become failures.

```csharp
// Entity found — wraps as success
User? user = await _dbContext.Users.FindAsync(userId);
OperationResult<User> result = user.ToOperationResult();
// user is null → Succeeded = false
// user exists → Succeeded = true, Result = user

// With strongly-typed errors on failure
OperationResult<User, List<string>> result = user.ToOperationResult<User, List<string>>(
    errors: new List<string> { "User not found" },
    message: "Lookup failed"
);
```

### Exception Handling with Custom Message Provider

Override the default exception message with something meaningful to your callers.

```csharp
public async Task<OperationResult<Report>> GenerateReportAsync(Guid reportId)
{
    return await OperationResultExtensions.TryOperationAsync<Report>(
        async () =>
        {
            var data = await _dataService.FetchAsync(reportId);
            var report = _reportBuilder.Build(data);
            return (report, default);
        },
        onException: ex => _logger.LogError(ex, "Report generation failed"),
        customMessageProvider: ex => $"Could not generate report {reportId}: {ex.Message}"
    );
}
// On exception: Succeeded = false, Message = "Could not generate report abc123: timeout"
```

### Sync Operations

Same pattern, no async. Works for in-memory logic, calculations, parsing.

```csharp
// Sync with result
OperationResult<int> parsed = OperationResultExtensions.TryOperation<int>(() =>
{
    var value = int.Parse(input);
    return (value, default);
});

// Sync with typed errors
OperationResult<Config, List<string>> config =
    OperationResultExtensions.TryOperation<Config, List<string>>(() =>
    {
        var cfg = ConfigParser.Parse(filePath);
        return (cfg, default);
    });

// Sync void — just success/failure, no result
OperationResult result = OperationResultExtensions.TryOperation(() =>
{
    _cache.Invalidate(key);
    return default; // errors — null means success
});
```

### Using OperationException for Domain-Specific Errors

Throw structured exceptions that carry error context, not just a message string.

```csharp
// Throw with typed errors
var errors = new List<ValidationError>
{
    new("Price", "Cannot be negative"),
    new("Name", "Required")
};

// Always throws if errors is ICollection with Count > 0
errors.Throw<List<ValidationError>>("Validation failed");

// Always throws regardless of content
errors.ThrowIfNullOrEmpty<List<ValidationError>>("Validation failed");

// Throw with just a message
OperationExceptionExtensions.Throw("Something broke");

// Catch and inspect
try
{
    errors.Throw<List<ValidationError>>("Validation failed");
}
catch (OperationException<List<ValidationError>> ex)
{
    List<ValidationError> typedErrors = ex.Errors; // strongly typed
    string message = ex.Message; // "Validation failed"
}
```

### Passing Results Between Layers

OperationResult flows naturally through your architecture. Each layer adds its own context without losing what came before.

**Endpoint (API layer):**

```csharp
app.MapPost("/users", async (CreateUserRequest req, UserService service) =>
{
    var result = await service.CreateUserAsync(req.Name, req.Email);

    return result.Succeeded
        ? Results.Created($"/users/{result.Result!.Id}", result.Result)
        : Results.BadRequest(new { result.Message, result.Errors });
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
        // Repository returns OperationResult — no exceptions to catch
        var result = await _repo.CreateUserAsync(name, email);

        if (!result.Succeeded)
            return result; // pass failure straight through

        // Fire-and-forget side effect — failures here don't affect the response
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
        return await OperationResultExtensions.TryOperationAsync<User>(async () =>
        {
            var user = new User(name, email);
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return (user, default);
        },
        customMessageProvider: ex => $"Failed to persist user: {ex.Message}");
    }
}
```

The endpoint doesn't know how the user was created. The service doesn't know what database is behind the repository. Each layer returns `OperationResult<User>`, and failures bubble up with full context.

## Requirements

- .NET 6+

## License

MIT
