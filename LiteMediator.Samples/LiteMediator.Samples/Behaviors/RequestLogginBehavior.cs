using LiteMediator.Behaviors;
using Microsoft.Extensions.Logging;

namespace LiteMediator.Samples.Behaviors;

public sealed class RequestLogginBehavior<TRequest, TResponse>(ILogger<RequestLogginBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    private readonly ILogger<RequestLogginBehavior<TRequest, TResponse>> logger = logger;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestExecutionDelegate<TResponse> next)
    {
        logger.LogInformation("INIT");
        TResponse result = await next();
        logger.LogInformation("END");

        return result;
    }
}