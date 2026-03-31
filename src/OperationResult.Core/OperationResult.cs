using System;
using System.Threading.Tasks;

namespace OperationResult.Core;

public class OperationResult<T> : OperationResultBase<OperationResult<T>, T, object>
{
    public static OperationResult<T> From(T? entity, string? errorMessage = default)
        => entity is null
            ? Fail(errorMessage ?? "Entity is null")
            : Ok(entity);
}

public class OperationResult<T, TErrors> : OperationResultBase<OperationResult<T, TErrors>, T, TErrors>
{
    public static OperationResult<T, TErrors> From(
        T entity,
        TErrors? errors = default,
        string? errorMessage = default)
        => entity is null || errors is not null
            ? Fail(errorMessage: errorMessage, errors: errors)
            : Ok(entity);
}

public class OperationResult : OperationResultBase<OperationResult, object, object>
{
    public static async Task<OperationResult> TryAsync(
        Func<Task> func,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            await func();
            return Ok();
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
            var customMessage = customMessageProvider?.Invoke(ex) ?? ex.Message;
            return Fail(customMessage);
        }
    }

    public static OperationResult Try(
        Action action,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            action();
            return Ok();
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
            var customMessage = customMessageProvider?.Invoke(ex) ?? ex.Message;
            return Fail(customMessage);
        }
    }
}
