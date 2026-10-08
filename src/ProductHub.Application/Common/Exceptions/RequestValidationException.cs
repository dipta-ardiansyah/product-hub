namespace ProductHub.Application.Common.Exceptions
{
    public class RequestValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public RequestValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred.")
        {
            Errors = errors;
        }

        public RequestValidationException(string propertyName, string errorMessage)
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>
            {
                { propertyName, new[] { errorMessage } }
            };
        }
    }
}