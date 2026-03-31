using System;
using System.Threading.Tasks;

namespace OperationResult.Core;

public abstract class OperationResultBase<TSelf, TResult, TErrors>
    where TSelf : OperationResultBase<TSelf, TResult, TErrors>, new()
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; protected set; }
    public TErrors? Errors { get; protected set; }
    public TResult? Result { get; protected set; }

    public static TSelf Fail(string? errorMessage = "Operation failed", TErrors? errors = default)
        => new TSelf { IsSuccess = false, ErrorMessage = errorMessage, Errors = errors };

    public static TSelf Ok(TResult? result = default)
        => new TSelf { IsSuccess = true, Result = result };

    public static async Task<TSelf> TryAsync(
        Func<Task<TResult>> func,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            var result = await func();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, exceptionHandler, customMessageProvider);
        }
    }

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
                return Fail(errors: errors);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, exceptionHandler, customMessageProvider);
        }
    }

    public static TSelf Try(
        Func<TResult> func,
        Action<Exception>? exceptionHandler = null,
        Func<Exception, string>? customMessageProvider = null)
    {
        try
        {
            var result = func();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, exceptionHandler, customMessageProvider);
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
                return Fail(errors: errors);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, exceptionHandler, customMessageProvider);
        }
    }

    private static TSelf HandleException(
        Exception ex,
        Action<Exception>? exceptionHandler,
        Func<Exception, string>? customMessageProvider)
    {
        exceptionHandler?.Invoke(ex);
        var customMessage = customMessageProvider?.Invoke(ex) ?? ex.Message;
        return new TSelf
        {
            IsSuccess = false,
            ErrorMessage = customMessage,
            Errors = ex is OperationBaseException<TErrors> opex ? opex.Errors : default
        };
    }
}
