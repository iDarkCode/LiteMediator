using LiteMediator.Behaviors;
using Microsoft.Extensions.Logging;

namespace LiteMediator.Samples.Behaviors;

public sealed class RequestLogginBehaviorDos<TRequest, TResponse>(ILogger<RequestLogginBehaviorDos<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    private readonly ILogger<RequestLogginBehaviorDos<TRequest, TResponse>> logger = logger;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestExecutionDelegate<TResponse> next)
    {
        logger.LogInformation("INIT 2");
        TResponse result = await next();
        logger.LogInformation("END 2");

        return result;
    }
}