namespace OperationResults.Tests;

public class OperationExceptionTest
{
    [Fact]
    public void OperationException_Constructor_SetsMessageAndErrors()
    {
        object errors = new List<string> { "Error 1", "Error 2" };

        var exception = new OperationException("Something went wrong", errors);

        exception.Message.Should().Be("Something went wrong");
        exception.Errors.Should().Be(errors);
    }

    [Fact]
    public void OperationException_Constructor_SetsMessage()
    {
        var exception = new OperationException("Something went wrong");

        exception.Message.Should().Be("Something went wrong");
        exception.Errors.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void OperationException_Constructor_AllowsEmptyNullOrWhitespaceMessage(string? message)
    {
        var exception = new OperationException(message);
        exception.Should().NotBeNull();
    }

    [Fact]
    public void OperationExceptionT_Constructor_SetsMessageAndErrors()
    {
        var exception = new OperationException<object>("Operation failed", new { ErrorCode = 1 });

        exception.Message.Should().Be("Operation failed");
        exception.Errors.Should().NotBeNull();
    }

    [Fact]
    public void OperationExceptionT_Constructor_SetsMessageOnly()
    {
        var exception = new OperationException<string>("Operation failed");

        exception.Message.Should().Be("Operation failed");
        exception.Errors.Should().BeNull();
    }

    [Fact]
    public void OperationExceptionT_ErrorsProperty_CanSetAndGet()
    {
        var newErrors = new { ErrorCode = 2 };
        var exception = new OperationException<object>("Operation failed", newErrors);

        exception.Errors.Should().Be(newErrors);
    }

    [Fact]
    public void InheritsFromException()
    {
        var exception = new OperationException("test");

        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void TypedInheritsFromException()
    {
        var exception = new OperationException<string>("test", "errors");

        exception.Should().BeAssignableTo<Exception>();
        exception.Should().BeAssignableTo<OperationBaseException<string>>();
    }
}
