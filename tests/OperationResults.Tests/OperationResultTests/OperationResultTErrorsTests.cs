namespace OperationResults.Tests;

public class OperationResultTErrorsTests
{
    [Fact]
    public void Ok_ReturnsSuccessTrue()
    {
        var result = OperationResult<string, string>.Ok("Success Result");

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Result.Should().Be("Success Result");
        result.ErrorMessage.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Fail_ReturnsSuccessFalse()
    {
        var result = OperationResult<string, string>.Fail("Operation failed", "Error 404");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Result.Should().BeNull();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public void Fail_SetsErrorsProperty()
    {
        var result = OperationResult<string, string>.Fail(errors: "Error 404");

        result.Errors.Should().Be("Error 404");
    }

    [Fact]
    public void Ok_ErrorsPropertyShouldBeNull()
    {
        var result = OperationResult<string, string>.Ok("Success Result");

        result.Errors.Should().BeNull();
    }

    // From

    [Fact]
    public void From_NullEntity_ReturnsFailure()
    {
        string entity = null!;

        var result = OperationResult<string, string>.From(entity);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void From_ValidEntity_ReturnsSuccess()
    {
        var result = OperationResult<string, string>.From("entity");

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("entity");
    }

    [Fact]
    public void From_WithErrors_ReturnsFailure()
    {
        var result = OperationResult<string, string>.From("entity", "some error");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Be("some error");
    }

    // TryAsync

    [Fact]
    public async Task TryAsync_Tuple_WithTypedOperationException_ReturnsTypedErrors()
    {
        Func<Task<(string, string?)>> func = () => throw new OperationException<string>("fail", "typed-error");

        var result = await OperationResult<string, string>.TryAsync(func);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("fail");
        result.Errors.Should().Be("typed-error");
    }

    [Fact]
    public async Task TryAsync_Tuple_NoException_ReturnsSuccess()
    {
        Func<Task<(string, string?)>> func = () => Task.FromResult<(string, string?)>(("data", default));

        var result = await OperationResult<string, string>.TryAsync(func);

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("data");
    }

    [Fact]
    public async Task TryAsync_Simple_NoException_ReturnsSuccess()
    {
        Func<Task<string>> func = () => Task.FromResult("data");

        var result = await OperationResult<string, string>.TryAsync(func);

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("data");
    }

    // Try

    [Fact]
    public void Try_Tuple_WithTypedOperationException_ReturnsTypedErrors()
    {
        Func<(string, string?)> func = () => throw new OperationException<string>("fail", "typed-error");

        var result = OperationResult<string, string>.Try(func);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Be("typed-error");
    }

    [Fact]
    public void Try_Simple_NoException_ReturnsSuccess()
    {
        Func<string> func = () => "data";

        var result = OperationResult<string, string>.Try(func);

        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be("data");
    }

    // ToDtoResponse

    [Fact]
    public void ToDtoResponse_Success_MapsCorrectly()
    {
        var result = OperationResult<string, List<string>>.Ok("data");

        var dto = result.ToDtoResponse(
            data => new { Value = data, Errors = (List<string>?)null },
            (errors, msg) => new { Value = (string?)null, Errors = errors });

        dto.Value.Should().Be("data");
        dto.Errors.Should().BeNull();
    }

    [Fact]
    public void ToDtoResponse_Failure_MapsWithTypedErrors()
    {
        var errors = new List<string> { "err1", "err2" };
        var result = OperationResult<string, List<string>>.Fail("Validation failed", errors);

        var dto = result.ToDtoResponse(
            data => new { Value = data, Errors = (List<string>?)null },
            (errs, msg) => new { Value = (string?)null, Errors = errs });

        dto.Value.Should().BeNull();
        dto.Errors.Should().BeEquivalentTo(errors);
    }
}
