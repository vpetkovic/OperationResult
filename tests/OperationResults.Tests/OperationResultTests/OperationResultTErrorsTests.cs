namespace OperationResults.Tests;

public class OperationResultTErrorsTests
{
    [Fact]
    public void IsSuccess_ReturnsOperationResultWithSuccessTrue()
    {
        var result = OperationResult<string, string>.IsSuccess("Success Result");

        result.Success.Should().BeTrue();
        result.Result.Should().Be("Success Result");
        result.ErrorMessage.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void IsFailure_ReturnsOperationResultWithSuccessFalse()
    {
        var result = OperationResult<string, string>.IsFailure("Error 404");

        result.Success.Should().BeFalse();
        result.Result.Should().BeNull();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().NotBeNull();
    }
}