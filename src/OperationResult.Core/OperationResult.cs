namespace OperationResult.Core;

public class OperationResult<T>
{
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    public object? Errors { get; private set; }
    
    public T? Result { get; private set; }

    public static OperationResult<T> IsFailure(object? errors, string? errorMessage = "Operation failed") 
        => new() { Success = false, ErrorMessage = errorMessage, Errors = errors};

    public static OperationResult<T> IsSuccess(T? result = default) 
        => new() { Success = true, Result = result };
}

public class OperationResult<T, TErrors>
{
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    public TErrors? Errors { get; private set; }
    
    public T? Result { get; private set; }

    public static OperationResult<T, TErrors> IsFailure(TErrors? errors, string? errorMessage = "Operation failed") 
        => new() { Success = false, ErrorMessage = errorMessage, Errors = errors};

    public static OperationResult<T, TErrors> IsSuccess(T? result = default) 
        => new() { Success = true, Result = result };
}

public class OperationResult
{
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    public object? Errors { get; private set; }

    public static OperationResult IsFailure(object? errors, string? errorMessage = "Operation failed") 
        => new() { Success = false, ErrorMessage = errorMessage, Errors = errors};

    public static OperationResult IsSuccess() 
        => new() { Success = true };
}