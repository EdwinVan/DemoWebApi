using System.Text.RegularExpressions;

namespace DemoWebApi.Services.Security;

public static class PasswordPolicy
{
    public static (bool IsValid, string Message) Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Password is required.");
        }

        if (password.Length < 8 || password.Length > 64)
        {
            return (false, "Password must be 8-64 characters.");
        }

        if (password.Any(char.IsWhiteSpace))
        {
            return (false, "Password cannot contain spaces.");
        }

        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            return (false, "Password must include at least one uppercase letter.");
        }

        if (!Regex.IsMatch(password, "[a-z]"))
        {
            return (false, "Password must include at least one lowercase letter.");
        }

        if (!Regex.IsMatch(password, "[0-9]"))
        {
            return (false, "Password must include at least one number.");
        }

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            return (false, "Password must include at least one special character.");
        }

        return (true, string.Empty);
    }
}
