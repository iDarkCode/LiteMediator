using LiteMediator.Abstractions;
using System.Runtime.CompilerServices;

namespace LiteMediator.Samples;

public record TestRequest(string Message) : IRequest<string>;

public class TestRequestHandler : IRequestHandler<TestRequest, string>
{
	public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
	{
		return Task.FromResult($"Echo: {request.Message}");
	}
}

public record TestNotification(string Info) : INotification;

public class TestNotificationHandler : INotificationHandler<TestNotification>
{
	public Task Handle(TestNotification notification, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Notification received: {notification.Info}");
		return Task.CompletedTask;
	}
}

public record TestStreamRequest(int Count) : IStreamRequest<int>;

public class TestStreamHandler : IStreamRequestHandler<TestStreamRequest, int>
{
	public async IAsyncEnumerable<int> Handle(TestStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		for (int i = 0; i < request.Count; i++)
		{
			yield return i;
			await Task.Delay(10, cancellationToken);
		}
	}
}