using System.ComponentModel.DataAnnotations;
using MediatR;
using ProductHub.Application.Common.Exceptions;

namespace ProductHub.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);

            if (!Validator.TryValidateObject(request, context, results, validateAllProperties: true))
            {
                var errors = results
                    .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : [string.Empty])
                        .Select(member => (Member: member, Message: r.ErrorMessage ?? "Invalid value.")))
                    .GroupBy(x => x.Member)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());

                throw new RequestValidationException(errors);
            }

            return await next();
        }
    }
}