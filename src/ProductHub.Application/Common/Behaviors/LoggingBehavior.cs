using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProductHub.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ctcancellationToken)
        {
            var name = typeof(TRequest).Name;
            var sw = Stopwatch.StartNew();
            // Sengaja TIDAK me-log isi request agar password/data sensitif tidak bocor.
            logger.LogInformation("Handling {RequestName}", name);
            try
            {
                var response = await next();
                logger.LogInformation("Handled {RequestName} in {ElapsedMs} ms", name, sw.ElapsedMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogWarning("{RequestName} failed after {ElapsedMs} ms: {ExceptionType}",
                    name, sw.ElapsedMilliseconds, ex.GetType().Name);
                throw;
            }
        }
    }
}