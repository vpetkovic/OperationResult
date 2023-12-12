namespace OperationResult.Core;

public class OperationResult<T> : OperationResultBase<OperationResult<T>, T, object>
{
    public static OperationResult<T> ToOperationResult(T? entity, string? errorMessage = default)
        => entity is null
            ? IsFailure(errorMessage ?? "Entity is null")
            : IsSuccess(entity);
}

public class OperationResult<T, TErrors> : OperationResultBase<OperationResult<T, TErrors>, T, TErrors>
{
    public static OperationResult<T, TErrors> ToOperationResult(
        T entity, 
        TErrors? errors = default, 
        string? errorMessage = default)
        => entity is null || errors is not null
            ? IsFailure(errors)
            : IsSuccess(entity);
}

public class OperationResult : OperationResultBase<OperationResult, object, object>
{
}