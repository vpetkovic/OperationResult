namespace OperationResults.Tests;

public class OperationResultTTests
{
    [Fact]
    public void IsSuccess_ReturnsOperationResultWithSuccessTrue()
    {
        var result = OperationResult<string>.IsSuccess("Success Result");

        result.Success.Should().BeTrue();
        result.Result.Should().Be("Success Result");
        result.ErrorMessage.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void IsFailure_ReturnsOperationResultWithSuccessFalse()
    {
        var result = OperationResult<string>.IsFailure(new { ErrorCode = "404" }, "Operation failed");

        result.Success.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().NotBeNull();
    }
    
    [Fact]
    public void IsFailure_SetsErrorsProperty()
    {
        // Arrange
        string expectedError = "Error 404";

        // Act
        var result = OperationResult<string, string>.IsFailure(expectedError, "Operation failed");

        // Assert
        result.Errors.Should().Be(expectedError);
    }

    [Fact]
    public void IsSuccess_ErrorsPropertyShouldBeNull()
    {
        // Act
        var result = OperationResult<string, string>.IsSuccess("Success Result");

        // Assert
        result.Errors.Should().BeNull();
    }
    
    [Fact]
    public async Task TryOperationAsync_T_WithOperationException()
    {
        var errors = new { ErrorCode = 1 };
        Func<Task<OperationResult<string>>> func 
            = () => throw new OperationException("Operation failed", errors);
            
        var result = await OperationResultExtensions.TryOperationAsync<string>(async () =>
        {
            await func();
            return (String.Empty, errors);
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public async Task TryOperationAsync_T_WithGeneralException_ExceptionHandlerNotInvoked()
    {
        Func<Task<OperationResult<string>>> func = 
            async () => throw new Exception("General exception");

        var result = await OperationResultExtensions.TryOperationAsync<string>(async () =>
        {
            await func();
            return (String.Empty, default);
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("General exception");
    }

    [Fact]
    public async Task TryOperationAsync_T_WithGeneralException_ExceptionHandlerInvoked()
    {
        bool handlerInvoked = false;
        void ExceptionHandler(Exception ex) => handlerInvoked = true;
        Func<Task<OperationResult<string>>> func = 
            async () => throw new Exception("General exception");

        var result = await OperationResultExtensions.TryOperationAsync<string>(async () =>
        {
            await func();
            return (String.Empty, default);
        }, exceptionHandler: ExceptionHandler);

        handlerInvoked.Should().BeTrue();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("General exception");
    }

    [Fact]
    public async Task TryOperationAsync_T_WithGeneralException_CustomMessageProviderInvoked()
    {
        string CustomMessageProvider(Exception ex) => "Custom message";
        Func<Task<OperationResult<string>>> func = 
            async () => throw new Exception("General exception");

        var result = await OperationResultExtensions.TryOperationAsync(async () =>
        {
            await func();
        }, customMessageProvider: CustomMessageProvider);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Custom message");
    }
    
    [Fact]
    public async Task TryOperationAsync_T_NoException_ReturnsSuccess()
    {
        Func<Task<(string, object?)>> func = async () => (String.Empty, default);

        var result = await OperationResultExtensions.TryOperationAsync<string>(func);

        result.Success.Should().BeTrue();
        result.Result.Should().BeEmpty();
        result.Errors.Should().BeNull();
    }
}