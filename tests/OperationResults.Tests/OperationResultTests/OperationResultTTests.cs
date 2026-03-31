namespace OperationResults.Tests;

public class OperationResultTTests
{
    // Factory methods

    [Fact]
    public void Ok_ReturnsSuccessTrue()
    {
        var result = OperationResult<string>.Ok("Success Result");

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Result.Should().Be("Success Result");
        result.ErrorMessage.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Fail_ReturnsSuccessFalse()
    {
        var result = OperationResult<string>.Fail("Operation failed", new { ErrorCode = "404" });

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Result.Should().BeNull();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public void Fail_MessageOnly_SetsMessageNotErrors()
    {
        var result = OperationResult<string>.Fail("Not found");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Not found");
        result.Errors.Should().BeNull();
    }

    // From (static)

    [Fact]
    public void From_NullEntity_ReturnsFailure()
    {
        string? entity = null;

        var result = OperationResult<string>.From(entity);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Entity is null");
    }

    [Fact]
    public void From_NullEntity_CustomMessage_ReturnsFailure()
    {
        string? entity = null;

        var result = OperationResult<string>.From(entity, "Not found");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Not found");
    }

    [Fact]
    public void From_ValidEntity_ReturnsSuccess()
    {
        var result = OperationResult<string>.From("hello");

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("hello");
    }

    // ToOperationResult (extension)

    [Fact]
    public void ToOperationResult_Extension_ValidEntity_ReturnsSuccess()
    {
        var result = "test".ToOperationResult();

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("test");
    }

    [Fact]
    public void ToOperationResult_Extension_NullEntity_ReturnsFailure()
    {
        string? entity = null;

        var result = entity.ToOperationResult("error");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("error");
    }

    // TryAsync (simple)

    [Fact]
    public async Task TryAsync_Simple_NoException_ReturnsSuccess()
    {
        var result = await OperationResult<string>.TryAsync(
            () => Task.FromResult("data"));

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("data");
    }

    [Fact]
    public async Task TryAsync_Simple_WithException_ReturnsFailure()
    {
        Func<Task<string>> func = () => throw new Exception("boom");

        var result = await OperationResult<string>.TryAsync(func);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("boom");
    }

    // TryAsync (tuple)

    [Fact]
    public async Task TryAsync_Tuple_NoException_ReturnsSuccess()
    {
        Func<Task<(string, object?)>> func = () => Task.FromResult<(string, object?)>(("data", default));

        var result = await OperationResult<string>.TryAsync(func);

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("data");
        result.Errors.Should().BeNull();
    }

    [Fact]
    public async Task TryAsync_Tuple_WithErrors_ReturnsFailure()
    {
        Func<Task<(string, object?)>> func = () => Task.FromResult<(string, object?)>(("", "Error"));

        var result = await OperationResult<string>.TryAsync(func);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Be("Error");
    }

    [Fact]
    public async Task TryAsync_Tuple_WithOperationException_ReturnsFailureWithErrors()
    {
        var errors = new { ErrorCode = 1 };
        Func<Task<(string, object?)>> func = () => throw new OperationException("Operation failed", errors);

        var result = await OperationResult<string>.TryAsync(func);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public async Task TryAsync_Simple_WithGeneralException_ReturnsFailure()
    {
        Func<Task<string>> func = () => throw new Exception("General exception");

        var result = await OperationResult<string>.TryAsync(func);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("General exception");
    }

    [Fact]
    public async Task TryAsync_Simple_ExceptionHandlerInvoked()
    {
        bool handlerInvoked = false;
        Func<Task<string>> func = () => throw new Exception("fail");

        var result = await OperationResult<string>.TryAsync(
            func, exceptionHandler: _ => handlerInvoked = true);

        handlerInvoked.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsync_Simple_CustomMessageProviderInvoked()
    {
        Func<Task<string>> func = () => throw new Exception("original");

        var result = await OperationResult<string>.TryAsync(
            func, customMessageProvider: _ => "Custom message");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Custom message");
    }

    // Try (simple)

    [Fact]
    public void Try_Simple_NoException_ReturnsSuccess()
    {
        var result = OperationResult<string>.Try(() => "data");

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("data");
    }

    [Fact]
    public void Try_Simple_WithException_ReturnsFailure()
    {
        Func<string> func = () => throw new Exception("boom");

        var result = OperationResult<string>.Try(func);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("boom");
    }

    // Try (tuple)

    [Fact]
    public void Try_Tuple_NoException_ReturnsSuccess()
    {
        Func<(string, object?)> func = () => ("data", null);

        var result = OperationResult<string>.Try(func);

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("data");
    }

    [Fact]
    public void Try_Tuple_WithErrors_ReturnsFailure()
    {
        Func<(string, object?)> func = () => ("", "Error");

        var result = OperationResult<string>.Try(func);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Be("Error");
    }

    [Fact]
    public void Try_Tuple_WithOperationException_ReturnsFailureWithErrors()
    {
        var errors = new { ErrorCode = 1 };
        Func<(string, object?)> func = () => throw new OperationException("Operation failed", errors);

        var result = OperationResult<string>.Try(func);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Try_Simple_WithGeneralException_ReturnsFailure()
    {
        Func<string> func = () => throw new Exception("General Failure");

        var result = OperationResult<string>.Try(func);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("General Failure");
    }

    [Fact]
    public void Try_Simple_ExceptionHandlerInvoked()
    {
        bool handlerInvoked = false;
        Func<string> func = () => throw new Exception("fail");

        var result = OperationResult<string>.Try(
            func, exceptionHandler: _ => handlerInvoked = true);

        handlerInvoked.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Try_Simple_CustomMessageProviderInvoked()
    {
        Func<string> func = () => throw new Exception("original");

        var result = OperationResult<string>.Try(
            func, customMessageProvider: _ => "Custom message");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Custom message");
    }

    // ToDtoResponse

    [Fact]
    public void ToDtoResponse_Success_MapsCorrectly()
    {
        var result = OperationResult<string>.Ok("data");

        var dto = result.ToDtoResponse(
            data => new { Value = data },
            error => new { Value = error ?? "" });

        dto.Value.Should().Be("data");
    }

    [Fact]
    public void ToDtoResponse_Failure_MapsCorrectly()
    {
        var result = OperationResult<string>.Fail("Something failed", "errors");

        var dto = result.ToDtoResponse(
            data => new { Error = (string?)null },
            error => new { Error = error });

        dto.Error.Should().Be("Something failed");
    }
}
