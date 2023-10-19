namespace OperationResults.Tests;

public class OperationExceptionTest
{
    [Fact]
    public void ThrowIfNullOrEmpty_WithErrors_ThrowsOperationException()
    {
        // Arrange
        string errorMessage = "Custom error message";
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act & Assert
        Action act = () => OperationExceptionExtensions.ThrowIfNullOrEmpty(errors, errorMessage);
        act.Should().Throw<OperationException>();
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WithoutErrors_ThrowsOperationException()
    {
        // Arrange
        List<string>? errors = null;
        var emptyErrors = new List<string>();

        // Act & Assert
        Action act = () => OperationExceptionExtensions.ThrowIfNullOrEmpty(errors);
        act.Should().Throw<OperationException>();
	
        Action act2 = () => OperationExceptionExtensions.ThrowIfNullOrEmpty(emptyErrors);
        act2.Should().Throw<OperationException>();
    }

    [Fact]
    public void Throw_WithErrors_ThrowOperationException()
    {
        // Arrange
        string errorMessage = "Custom error message";
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act & Assert
        Action act = () => OperationExceptionExtensions.Throw(errors, errorMessage);
        act.Should().Throw<OperationException>();
    }

    [Fact]
    public void Throw_WithoutErrors_DoesntThrowOperationException()
    {
        // Arrange
        string errorMessage = "Custom error message";
        var errors = new List<string>();

        // Act & Assert
        Action act = () => OperationExceptionExtensions.Throw(errors, errorMessage);
        act.Should().NotThrow();
    }
    
    [Fact]
    public void Throw_WithoutErrors_ThrowsOperationExceptionWithDefaultMessageAndNullErrors()
    {
        // Act & Assert
        Action act = () => OperationExceptionExtensions.Throw();
        act.Should().Throw<OperationException>();
    }
    
    [Fact]
    public void OperationException_Constructor_ShouldSetMessageAndErrors()
    {
        // Arrange
        string message = "Something went wrong";
        object errors = new List<string> { "Error 1", "Error 2" };
        
        // Act & Assert
        var exception = new OperationException(message, errors);
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrWhiteSpace().And.Be(message);
        exception.Errors.Should().NotBeNull().And.Be(errors);
    }

    [Fact]
    public void OperationException_Constructor_ShouldSetMessage()
    {
        string message = "Something went wrong";
        var exception = new OperationException(message);
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrWhiteSpace().And.Be(message);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void OperationException_Constructor_ShouldAlloEmptyNullOrWhitespaceMessage(string message)
    {
        var exception = new OperationException(message);
        exception.Should().NotBeNull();
    }
    
    [Fact]
    public void Constructor_WithMessageAndErrors_SetsBoth()
    {
        // Arrange
        var expectedMessage = "Operation failed";
        var expectedErrors = new { ErrorCode = 1 };
        
        // Act
        var exception = new OperationException<object>(expectedMessage, expectedErrors);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.Errors.Should().Be(expectedErrors);
    }

    [Fact]
    public void Constructor_WithMessageAndDefaultErrors_SetsBoth()
    {
        // Arrange
        var expectedMessage = "Operation failed";

        // Act
        var exception = new OperationException<string>(expectedMessage);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.Errors.Should().BeNull();
    }

    [Fact]
    public void ErrorsProperty_CanSetAndGet()
    {
        // Arrange
        var newErrors = new { ErrorCode = 2 };
        var exception = new OperationException<object>("Operation failed", newErrors);
        
        // Assert
        exception.Errors.Should().Be(newErrors);
    }
}