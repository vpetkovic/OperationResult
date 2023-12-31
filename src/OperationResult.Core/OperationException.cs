﻿namespace OperationResult.Core;

public class OperationException : Exception
{
    public object? Errors { get; set; }
		
    public OperationException(string? message, object? errors) : base(message)
    {
        Errors = errors;
    }
		
    public OperationException(string message) : base(message)
    { }
}

public class OperationException<TErrors> : Exception
{
    public TErrors? Errors { get; set; }
		
    public OperationException(string message, TErrors? errors) : base(message)
    {
        Errors = errors;
    }
		
    public OperationException(string message) : base(message)
    { }
}