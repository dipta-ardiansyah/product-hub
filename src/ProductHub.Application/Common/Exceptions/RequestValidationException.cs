namespace ProductHub.Application.Common.Exceptions
{
    public class RequestValidationException(IDictionary<string, string[]> errors)
        : Exception("One or more validation errors occurred.")
    {
        public IDictionary<string, string[]> Errors { get; } = errors;
    }
}