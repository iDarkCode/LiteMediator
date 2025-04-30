namespace LiteMediator.Behaviors;

public interface IPipelineBehavior<TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, 
        RequestExecutionDelegate<TResponse> next);
}