using OperationResult.Core;
using OpResult = OperationResult.Core.OperationResult;

namespace OperationResults.Tests;

public class OperationResultTests
{
    [Fact]
    public void Ok_ReturnsSuccessTrue()
    {
        var result = OpResult.Ok();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Fail_ReturnsSuccessFalse()
    {
        var result = OpResult.Fail("Operation failed", new { ErrorCode = "404" });

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Operation failed");
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public void Fail_MessageOnly()
    {
        var result = OpResult.Fail("Something went wrong");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Something went wrong");
        result.Errors.Should().BeNull();
    }

    // TryAsync

    [Fact]
    public async Task TryAsync_NoException_ReturnsSuccess()
    {
        var result = await OpResult.TryAsync(
            () => Task.CompletedTask);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsync_WithException_ReturnsFailure()
    {
        var result = await OpResult.TryAsync(
            () => throw new Exception("Exception occurred"));

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Exception occurred");
    }

    [Fact]
    public async Task TryAsync_ExceptionHandlerInvoked()
    {
        bool handlerInvoked = false;

        var result = await OpResult.TryAsync(
            () => throw new Exception("fail"),
            exceptionHandler: _ => handlerInvoked = true);

        handlerInvoked.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsync_CustomMessageProviderInvoked()
    {
        var result = await OpResult.TryAsync(
            () => throw new Exception("original"),
            customMessageProvider: _ => "Custom Message");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Custom Message");
    }

    [Fact]
    public async Task TryAsync_WithBothHandlers()
    {
        bool handlerInvoked = false;

        var result = await OpResult.TryAsync(
            () => throw new Exception("fail"),
            exceptionHandler: _ => handlerInvoked = true,
            customMessageProvider: _ => "Custom message");

        handlerInvoked.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Custom message");
    }

    // Try

    [Fact]
    public void Try_Successful()
    {
        var result = OpResult.Try(() => { });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Try_WithException_ReturnsFailure()
    {
        var result = OpResult.Try(
            () => throw new Exception("General Failure"));

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("General Failure");
    }

    [Fact]
    public void Try_ExceptionHandlerInvoked()
    {
        bool handlerInvoked = false;

        var result = OpResult.Try(
            () => throw new Exception("fail"),
            exceptionHandler: _ => handlerInvoked = true);

        handlerInvoked.Should().BeTrue();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Try_CustomMessageProviderInvoked()
    {
        var result = OpResult.Try(
            () => throw new Exception("original"),
            customMessageProvider: _ => "Custom message");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Custom message");
    }
}
