namespace OperationResult.Core;

public abstract class OperationResultBase<TSelf, TResult, TErrors>
    where TSelf : OperationResultBase<TSelf, TResult, TErrors>, new()
{
    public bool Success { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public TErrors? Errors { get; protected set; }
    public TResult? Result { get; protected set; }

    public static TSelf IsFailure(TErrors? errors, string? errorMessage = "Operation failed") 
        => new TSelf { Success = false, ErrorMessage = errorMessage, Errors = errors };

    public static TSelf IsSuccess(TResult? result = default) 
        => new TSelf { Success = true, Result = result };
    
    public static async Task<TSelf> TryAsync(
        Func<Task<(TResult, TErrors?)>> func,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            var (result, errors) = await func();
            if (errors != null) 
            {
                return IsFailure(errors);
            }
            return IsSuccess(result);
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
            var customMessage = customMessageProvider?.Invoke(ex) ?? ex.Message;
            return new TSelf
            {
                Success = false, 
                ErrorMessage = customMessage,
                Errors = ex is OperationException<TErrors> opex ? opex.Errors : default
            };
        }
    }
    
    public static TSelf Try(
        Func<(TResult, TErrors?)> func,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            var (result, errors) = func();
            if (errors != null) 
            {
                return IsFailure(errors);
            }
            return IsSuccess(result);
        }
        catch (Exception ex)
        {
            exceptionHandler?.Invoke(ex);
            var customMessage = customMessageProvider?.Invoke(ex) ?? ex.Message;
            return new TSelf
            {
                Success = false, 
                ErrorMessage = customMessage,
                Errors = ex is OperationException<TErrors> opex ? opex.Errors : default
            };
        }
    }
}