using System.Collections;

namespace OperationResult.Core;

public static class OperationExceptionExtensions
{
    public static void ThrowIfNullOrEmpty<TErrors>(TErrors errors, string? message = "Operation Failed") 
    {
        throw new OperationException(message, errors);
    }
    
    public static void Throw<TErrors>(TErrors? errors, string? message = "Operation Failed") 
    {
        if (errors is ICollection { Count: > 0 })
        {
            throw new OperationException(message, errors);
        }
    }
		
    public static void Throw(string? message = "Operation Failed") 
    {
        throw new OperationException(message, default);
    }
}