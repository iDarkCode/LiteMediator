using LiteMediator.Abstractions;
using System.Runtime.CompilerServices;

namespace LiteMediator.Tests.Fixtures;


public class TestRequest : IRequest<string>
{
    public string Name { get; }

    public TestRequest(string name)
    {
        Name = name;
    }
}

public class TestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Hello: {request.Name}!");
    }
}

public class TestNotification : INotification
{
    public string Message { get; }

    public TestNotification(string message)
    {
        Message = message;
    }
}


public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public List<string> ReceivedMessages { get; } = new();

    public Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        ReceivedMessages.Add(notification.Message);
        return Task.CompletedTask;
    }
}


public class TestStreamRequest : IStreamRequest<string>
{
    public int Count { get; }

    public TestStreamRequest(int count)
    {
        Count = count;
    }
}


public class TestStreamRequestHandler : IStreamRequestHandler<TestStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(TestStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.Count; i++)
        {
            yield return $"Item {i + 1}";
            await Task.Delay(10, cancellationToken); // simular algo de trabajo
        }
    }
}
