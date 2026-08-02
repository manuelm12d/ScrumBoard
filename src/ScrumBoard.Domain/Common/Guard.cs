namespace ScrumBoard.Domain.Common;

internal static class Guard
{
    internal static string AgainstNullOrWhiteSpace(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"El campo '{field}' es obligatorio.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainValidationException($"El campo '{field}' no puede superar {maxLength} caracteres.");
        }

        return trimmed;
    }

    internal static string? AgainstTooLong(string? value, string field, int maxLength)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainValidationException($"El campo '{field}' no puede superar {maxLength} caracteres.");
        }

        return trimmed.Length == 0 ? null : trimmed;
    }
}
