namespace OperationResult.Core;

public static class OperationResultExtensions
{
    public static OperationResult<T> ToOperationResult<T>(this T? entity, string? errorMessage = default)
        => entity is null
            ? OperationResult<T>.IsFailure(errorMessage ?? "Entity is null")
            : OperationResult<T>.IsSuccess(entity);
    
    public static OperationResult<T> ToOperationResult<T, TErrors>(
        this T entity, 
        TErrors? errors = default, 
        string? errorMessage = default)
        => entity is null || errors is not null
            ? OperationResult<T>.IsFailure(errors)
            : OperationResult<T>.IsSuccess(entity);
    
    public static async Task<OperationResult<T>> TryOperationAsync<T>(
        Func<Task<(T, object?)>> func,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            var (result, errors) = await func();
            if (errors != null) 
            {
                return OperationResult<T>.IsFailure(errors);
            }
            return OperationResult<T>.IsSuccess(result);
        }
        catch (OperationException opex) 
        {
            return OperationResult<T>.IsFailure(opex.Errors, opex.Message);
        }
        catch (Exception e)
        {
            exceptionHandler?.Invoke(e);
            var customMessage = customMessageProvider?.Invoke(e) ?? e.Message;
            return OperationResult<T>.IsFailure(default, customMessage);
        }
    }
    
    public static async Task<OperationResult> TryOperationAsync(
        Func<Task> func,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            await func();
            return OperationResult.IsSuccess();
        }
        catch (Exception e)
        {
            exceptionHandler?.Invoke(e);
            var customMessage = customMessageProvider?.Invoke(e) ?? e.Message;
            return OperationResult.IsFailure(default, customMessage);
        }
    }
    
    public static OperationResult<T> TryOperation<T>(
        Func<(T, object?)> func,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            var (result, errors) = func();
            if (errors != null) 
            {
                return OperationResult<T>.IsFailure(errors);
            }
            return OperationResult<T>.IsSuccess(result);
        }
        catch (OperationException opex) 
        {
            return OperationResult<T>.IsFailure(opex.Errors, opex.Message);
        }
        catch (Exception e)
        {
            exceptionHandler?.Invoke(e);
            var customMessage = customMessageProvider?.Invoke(e) ?? e.Message;
            return OperationResult<T>.IsFailure(default, customMessage);
        }
    }
    
    public static OperationResult TryOperation(
        Action action,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            action();
            return OperationResult.IsSuccess();
        }
        catch (Exception e)
        {
            exceptionHandler?.Invoke(e);
            var customMessage = customMessageProvider?.Invoke(e) ?? e.Message;
            return OperationResult.IsFailure(default, errorMessage: customMessage);
        }
    }
}