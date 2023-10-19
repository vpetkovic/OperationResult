namespace OperationResults.Tests;
using OperationResult.Core;

public class OperationResultTests
{
    [Fact]
    void ShouldConvertObjectToOperationResult()
    {
        // Arrange
        var result = "test".ToOperationResult();

        // Act & Assert
        result.Should().BeOfType(result.GetType());
        result.Result.Should().BeOfType(result.Result?.GetType());
        result.Success.Should().BeTrue();
    }
    
    [Fact]
    public void IsSuccess_ReturnsOperationResultWithSuccessTrue()
    {
        var result = OperationResult.IsSuccess();

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void IsFailure_ReturnsOperationResultWithSuccessFalse()
    {
        var result = OperationResult.IsFailure(new { ErrorCode = "404" });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().NotBeNull();
    }
    
    [Fact]
    public void ToOperationResult_NullEntity_ReturnsFailure()
    {
        string? entity = null;
        const string? errors = "error";

        var result = entity.ToOperationResult(errors);

        result.Success.Should().BeFalse();
        result.Errors.Should().Be(errors);
    }

    [Fact]
    public void ToOperationResult_ValidEntity_ReturnsSuccess()
    {
        const string entity = "entity";
        
        var result = entity.ToOperationResult<string, string>();

        result.Success.Should().BeTrue();
        result.Result.Should().Be(entity);
    }
    
     [Fact]
    public async Task TryOperationAsyncT_Successful()
    {
        Func<Task<OperationResult<string>>> func =
            async () => await Task.FromResult("Result".ToOperationResult());

        var result = await OperationResultExtensions.TryOperationAsync<string>(async () =>
        {
            var result = await func();
            return (result.Result!, null);
        });

        result.Success.Should().BeTrue();
        result.Result.Should().Be("Result");
    }

    [Fact]
    public async Task TryOperationAsyncT_WithErrors()
    {
        Func<Task<OperationResult<string>>> func = 
            async () => await Task.FromResult(OperationResult<string>.IsFailure("Error"));

        var result = await OperationResultExtensions.TryOperationAsync(async () =>
        {
            var result = await func();
            return (result.Result!, "Error");
        });

        result.Success.Should().BeFalse();
        result.Errors.Should().Be("Error");
    }

    [Fact]
    public async Task TryOperationAsyncT_OperationException()
    {
        Func<Task<OperationResult<string>>> func = 
            async () => await Task.FromResult(OperationResult<string>.IsFailure("Error"));
        
        var result = await OperationResultExtensions.TryOperationAsync(async () =>
        {
            var result = await func();
            return (result.Result!, result.Errors);
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().Be("Error");
    }

    [Fact]
    public async Task TryOperationAsyncT_GeneralException()
    {
        Func<Task<OperationResult<string>>> func = 
            () => throw new Exception("General Failure");

        var result = await OperationResultExtensions.TryOperationAsync<string>(async () =>
        {
            var result = await func();
            return (result.Result!, null);
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("General Failure");
    }
    
    [Fact]
    public async Task TryOperationAsync_ExceptionHandlerInvoked()
    {
        Func<Task> func = () => throw new Exception("General Failure");
        bool exceptionHandlerInvoked = false;
        void ExceptionHandler(Exception ex) => exceptionHandlerInvoked = true;

        var result = await OperationResultExtensions
            .TryOperationAsync(func, exceptionHandler: ExceptionHandler);

        exceptionHandlerInvoked.Should().BeTrue();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task TryOperationAsync_CustomMessageProviderInvoked()
    {
        Func<Task> func = () => throw new Exception("General Failure");
        bool customMessageProviderInvoked = false;

        string CustomMessageProvider(Exception ex)
        {
            customMessageProviderInvoked = true;
            return "Custom Message";
        }

        var result = await OperationResultExtensions
            .TryOperationAsync(func, customMessageProvider: CustomMessageProvider);

        customMessageProviderInvoked.Should().BeTrue();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Custom Message");
    }
    
    [Fact]
    public async Task TryOperationAsync_NoException_Success()
    {
        Func<Task> func = async () => await Task.CompletedTask;

        var result = await OperationResultExtensions
            .TryOperationAsync(func);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task TryOperationAsync_WithException_NoHandlers()
    {
        Func<Task> func = () => throw new Exception("Exception occurred");

        var result = await OperationResultExtensions
            .TryOperationAsync(func);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Exception occurred");
    }

    [Fact]
    public async Task TryOperationAsync_WithException_WithExceptionHandler()
    {
        bool exceptionHandled = false;
        void ExceptionHandler(Exception ex) => exceptionHandled = true;
        Func<Task> func = () => throw new Exception("Exception occurred");

        var result = await OperationResultExtensions
            .TryOperationAsync(func, exceptionHandler: ExceptionHandler);

        result.Success.Should().BeFalse();
        exceptionHandled.Should().BeTrue();
    }

    [Fact]
    public async Task TryOperationAsync_WithException_WithCustomMessageProvider()
    {
        string CustomMessageProvider(Exception ex) => "Custom message";
        Func<Task> func = () => throw new Exception("Exception occurred");

        var result = await OperationResultExtensions
            .TryOperationAsync(func, customMessageProvider: CustomMessageProvider);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Custom message");
    }

    [Fact]
    public async Task TryOperationAsync_WithException_WithExceptionHandler_And_CustomMessageProvider()
    {
        bool exceptionHandled = false;
        void ExceptionHandler(Exception ex) => exceptionHandled = true;
        string CustomMessageProvider(Exception ex) => "Custom message";
        Func<Task> func = () => throw new Exception("Exception occurred");

        var result = await OperationResultExtensions
            .TryOperationAsync(func, 
                exceptionHandler: ExceptionHandler,
                customMessageProvider: CustomMessageProvider);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Custom message");
        exceptionHandled.Should().BeTrue();
    }
    
    [Fact]
    public void TryOperationT_Successful()
    {
        Func<OperationResult<string>> func = () => "Result".ToOperationResult();

        var result = OperationResultExtensions.TryOperation<string>(() =>
        {
            var result = func();
            return (result.Result!, null);
        });

        result.Success.Should().BeTrue();
        result.Result.Should().Be("Result");
    }

    [Fact]
    public void TryOperationT_WithErrors()
    {
        Func<OperationResult<string>> func = 
            () => OperationResult<string>.IsFailure("Error");

        var result = OperationResultExtensions.TryOperation(() =>
        {
            var result = func();
            return (result.Result!, "Error");
        });

        result.Success.Should().BeFalse();
        result.Errors.Should().Be("Error");
    }

    [Fact]
    public void TryOperationT_OperationException()
    {
        Func<OperationResult<string>> func = 
            () => OperationResult<string>.IsFailure("Error");
        
        var result = OperationResultExtensions.TryOperation(() =>
        {
            var result = func();
            return (result.Result!, result.Errors);
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().Be("Error");
    }

    [Fact]
    public void TryOperationT_GeneralException()
    {
        Func<OperationResult<string>> func = 
            () => throw new Exception("General Failure");

        var result = OperationResultExtensions.TryOperation<string>(() =>
        {
            var result = func();
            return (result.Result!, null);
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("General Failure");
    }
    
    [Fact]
    public void TryOperation_T_WithOperationException()
    {
        Func<OperationResult<string>> func = 
            () => throw new OperationException("Operation failed", new { ErrorCode = 1 });

        var result = OperationResultExtensions.TryOperation<string>(() =>
        {
            var result = func();
            return (result.Result!, null);
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().BeEquivalentTo(new { ErrorCode = 1 });
    }

    [Fact]
    public void TryOperation_T_WithGeneralException_ExceptionHandlerNotInvoked()
    {
        Func<OperationResult<string>> func = 
            () => throw new Exception("General exception");

        var result = OperationResultExtensions.TryOperation<string>(() =>
        {
            var result = func();
            return (result.Result!, null);
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("General exception");
    }

    [Fact]
    public void TryOperation_T_WithGeneralException_ExceptionHandlerInvoked()
    {
        bool handlerInvoked = false;
        void ExceptionHandler(Exception ex) => handlerInvoked = true;
        Func<OperationResult<string>> func = 
            () => throw new Exception("General exception");

        var result = OperationResultExtensions.TryOperation<string>(() =>
        {
            var result = func();
            return (result.Result!, null);
        }, exceptionHandler: (Action<Exception>)ExceptionHandler);

        handlerInvoked.Should().BeTrue();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("General exception");
    }

    [Fact]
    public void TryOperation_T_WithGeneralException_CustomMessageProviderInvoked()
    {
        string CustomMessageProvider(Exception ex) => "Custom message";
        Func<OperationResult<string>> func = 
            () => throw new Exception("General exception");

        var result = OperationResultExtensions.TryOperation<string>(() =>
        {
            var result = func();
            return (result.Result!, null);
        }, customMessageProvider: CustomMessageProvider);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Custom message");
    }

    [Fact]
    public void TryOperation_Successful()
    {
        Action action = () => { /* do nothing */ };

        var result = OperationResultExtensions.TryOperation(action);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void TryOperation_GeneralException()
    {
        Action action = () => throw new Exception("General Failure");

        var result = OperationResultExtensions.TryOperation(action);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("General Failure");
    }
}