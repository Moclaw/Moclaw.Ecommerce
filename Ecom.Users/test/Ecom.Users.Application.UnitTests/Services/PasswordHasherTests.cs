using Ecom.Users.Application.Services;
using FluentAssertions;

namespace Ecom.Users.Application.UnitTests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        
        // Verify the format: {iterations}:{salt}:{hash}
        var parts = hashedPassword.Split(':');
        parts.Should().HaveCount(3);
        
        // Verify iterations part
        int.TryParse(parts[0], out var iterations).Should().BeTrue();
        iterations.Should().Be(100000);
        
        // Verify salt and hash are base64 encoded
        IsValidBase64(parts[1]).Should().BeTrue("Salt should be valid base64");
        IsValidBase64(parts[2]).Should().BeTrue("Hash should be valid base64");
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "Each hash should have a unique salt");
        
        // But both should verify against the original password
        _passwordHasher.VerifyPassword(password, hash1).Should().BeTrue();
        _passwordHasher.VerifyPassword(password, hash2).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "TestPassword123!";
        var incorrectPassword = "WrongPassword456!";
        var hashedPassword = _passwordHasher.HashPassword(correctPassword);

        // Act
        var isValid = _passwordHasher.VerifyPassword(incorrectPassword, hashedPassword);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidHashFormat_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "invalid:hash:format:too:many:parts";

        // Act
        var isValid = _passwordHasher.VerifyPassword(password, invalidHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidIterationCount_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "notanumber:validbase64salt:validbase64hash";

        // Act
        var isValid = _passwordHasher.VerifyPassword(password, invalidHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidBase64Salt_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "100000:invalidbase64!@#$:dmFsaWRiYXNlNjRoYXNo";

        // Act
        var isValid = _passwordHasher.VerifyPassword(password, invalidHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidBase64Hash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "100000:dmFsaWRiYXNlNjRzYWx0:invalidbase64!@#$";

        // Act
        var isValid = _passwordHasher.VerifyPassword(password, invalidHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("short")]
    [InlineData("ThisIsAVeryLongPasswordThatShouldStillWorkProperly123!@#")]
    public void HashPassword_WithVariousPasswordLengths_ShouldWorkCorrectly(string password)
    {
        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);
        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Password123!")]
    [InlineData("simplepassword")]
    [InlineData("UPPERCASE")]
    [InlineData("lowercase")]
    [InlineData("123456789")]
    [InlineData("!@#$%^&*()")]
    [InlineData("PasswordWithUnicode🔒")]
    [InlineData("   spaces   ")]
    public void HashAndVerifyPassword_WithVariousCharacters_ShouldWorkCorrectly(string password)
    {
        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);
        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        isValid.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_ShouldUseSecureDefaults()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        var parts = hashedPassword.Split(':');
        
        // Verify iteration count is secure (100,000)
        var iterations = int.Parse(parts[0]);
        iterations.Should().Be(100000, "Should use a secure number of iterations");
        
        // Verify salt length (16 bytes = 24 base64 chars with padding)
        var saltBytes = Convert.FromBase64String(parts[1]);
        saltBytes.Length.Should().Be(16, "Salt should be 128 bits (16 bytes)");
        
        // Verify hash length (32 bytes = 44 base64 chars with padding)  
        var hashBytes = Convert.FromBase64String(parts[2]);
        hashBytes.Length.Should().Be(32, "Hash should be 256 bits (32 bytes)");
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldHandleGracefully()
    {
        // Arrange
        var emptyPassword = "";
        var hashedPassword = _passwordHasher.HashPassword(emptyPassword);

        // Act
        var isValidEmpty = _passwordHasher.VerifyPassword(emptyPassword, hashedPassword);
        var isValidNonEmpty = _passwordHasher.VerifyPassword("nonempty", hashedPassword);

        // Assert
        isValidEmpty.Should().BeTrue();
        isValidNonEmpty.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_PerformanceTest_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var password = "TestPassword123!";
        var startTime = DateTime.UtcNow;

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);
        var endTime = DateTime.UtcNow;

        // Assert
        var duration = endTime - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(5), "Hashing should complete in reasonable time");
        hashedPassword.Should().NotBeNullOrEmpty();
    }

    private static bool IsValidBase64(string value)
    {
        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
