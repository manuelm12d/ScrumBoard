namespace ScrumBoard.Application.Common;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base($"No se encontró {resource} con identificador '{key}'.")
    {
    }
}

public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Credenciales inválidas.")
    {
    }
}
