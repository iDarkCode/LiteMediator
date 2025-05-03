namespace LiteMediator.Behaviors;

public interface IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, 
        RequestExecutionDelegate<TResponse> next);
}