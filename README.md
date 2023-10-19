[![Build and Push NuGet Package](https://github.com/vpetkovic/OperationResult/actions/workflows/build-nuget-push.yml/badge.svg?branch=develop)](https://github.com/vpetkovic/OperationResult/actions/workflows/build-nuget-push.yml)

| Package | NuGet | Version | Stats |
| --------------- | --------------- | --------------- | --------------- |
| `OperationResult.Core` | [`Install-Package OperationResult.Core`](https://www.nuget.org/packages/OperationResult.Core/) | ![Nuget](https://img.shields.io/nuget/v/OperationResult.Core) | ![Nuget](https://img.shields.io/nuget/dt/OperationResult.Core?label=%20Downloads)

# OperationResult
Provides consistent, strongly-typed generic return objects throughout different layers of an application, from database queries in repositories to service-level operations

Usage Example
```csharp
public record User(string name);

(User, List<Error>) createdUser = await OperationResultExtensions.TryOperationAsync<User>(async () => 
{
  var user = new User("John");
  
  await _someDbContext.Users.Add(user);
  await _someDbContext.SaveChangesAsync();
  
  return (user, default);
}, onException => Rollback());
```

- [ ] Add More usage examples
