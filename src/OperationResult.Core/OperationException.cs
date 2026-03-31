using System;

namespace OperationResult.Core;

public class OperationException : OperationBaseException<object>
{
    public OperationException(string? message, object? errors) : base(message, errors)
    {
    }

    public OperationException(string? message) : base(message)
    {
    }
}

public class OperationException<TErrors> : OperationBaseException<TErrors>
{
    public OperationException(string? message, TErrors? errors) : base(message, errors)
    {
    }

    public OperationException(string? message) : base(message)
    {
    }
}

public abstract class OperationBaseException<TErrors> : Exception
{
    public TErrors? Errors { get; set; }

    protected OperationBaseException(string? message, TErrors? errors) : base(message)
    {
        Errors = errors;
    }

    protected OperationBaseException(string? message) : base(message)
    {
    }
}
