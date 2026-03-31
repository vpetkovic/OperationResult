using System;

namespace OperationResult.Core;

public static class OperationResultExtensions
{
    public static OperationResult<T> ToOperationResult<T>(this T? entity, string? errorMessage = default)
        => entity is null
            ? OperationResult<T>.Fail(errorMessage ?? "Entity is null")
            : OperationResult<T>.Ok(entity);

    public static TResponse ToDtoResponse<T, TResponse>(
        this OperationResult<T> operationResult,
        Func<T, TResponse> successMapping,
        Func<string?, TResponse> failureMapping)
    {
        return operationResult.IsSuccess
            ? successMapping(operationResult.Result!)
            : failureMapping(operationResult.ErrorMessage);
    }

    public static TResponse ToDtoResponse<T, TErrors, TResponse>(
        this OperationResult<T, TErrors> operationResult,
        Func<T, TResponse> successMapping,
        Func<TErrors?, string?, TResponse> failureMapping)
    {
        return operationResult.IsSuccess
            ? successMapping(operationResult.Result!)
            : failureMapping(operationResult.Errors, operationResult.ErrorMessage);
    }
}
