namespace ProductHub.Application.Common.Exceptions
{
    public class NotFoundException(string name, object key)
        : Exception($"{name} with id '{key}' was not found.");

    public class ConflictException(string message) : Exception(message);

    public class UnauthorizedException(string message) : Exception(message);

    public class RequestValidationException(IDictionary<string, string[]> errors)
        : Exception("One or more validation errors occurred.")
    {
        public IDictionary<string, string[]> Errors { get; } = errors;
    }
}