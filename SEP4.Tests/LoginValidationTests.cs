namespace SEP4.Tests;

public class LoginValidationTests
{
    [Fact]
    public void PasswordTooShort_ShouldFail()
    {
        string password = "123";

        bool valid = password.Length >= 6;

        Assert.False(valid);
    }

    [Fact]
    public void PasswordLongEnough_ShouldPass()
    {
        string password = "123456";

        bool valid = password.Length >= 6;

        Assert.True(valid);
    }

    [Fact]
    public void EmptyPassword_ShouldFail()
    {
        string password = "";

        bool valid = password.Length >= 6;

        Assert.False(valid);
    }

    [Fact]
    public void InvalidEmail_ShouldFail()
    {
        string email = "notAnEmail";

        bool valid = email.Contains("@");

        Assert.False(valid);
    }

    [Fact]
    public void EmptyUsername_ShouldFail()
    {
        string username = "";

        bool valid = !string.IsNullOrWhiteSpace(username);

        Assert.False(valid);
    }

    [Fact]
    public void ValidEmail_ShouldPass()
    {
        string email = "test@test.com";

        bool valid = email.Contains("@");

        Assert.True(valid);
    }

    [Fact]
    public void ValidUsername_ShouldPass()
    {
        string username = "Attila";

        bool valid = !string.IsNullOrWhiteSpace(username);

        Assert.True(valid);
    }
}