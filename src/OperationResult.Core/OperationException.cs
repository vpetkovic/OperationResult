using System.Collections;

namespace OperationResult.Core;

public class OperationException : OperationBaseException<object>
{
    public OperationException(string message, object? errors) : base(message, errors)
    {
    }

    public OperationException(string message) : base(message)
    {
    }
}

public class OperationException<TErrors> : OperationBaseException<TErrors>
{
    public OperationException(string message, TErrors? errors) : base(message, errors)
    {
    }

    public OperationException(string message) : base(message)
    {
    }
}


public abstract class OperationBaseException<TErrors> : Exception
{
    public TErrors? Errors { get; set; }
		
    public OperationBaseException(string message, TErrors? errors) : base(message)
    {
        Errors = errors;
    }
		
    public OperationBaseException(string message) : base(message)
    { }
    
    public static void ThrowIfErrorsExist<T>(T errors, string message = "Operation Failed") 
    {
        if (errors != null || IsNonEmptyCollection(errors))
        {
            throw new OperationException(message, errors);
        }
    }
    public static void Throw(string? message = "Operation Failed") 
        => throw new OperationException(message, default);

    private static bool IsNonEmptyCollection<TErrors>(TErrors errors) 
        => errors is ICollection { Count: > 0 };
}