using Bogus;
using LiteMediator.Abstractions;
using LiteMediator.Core;
using LiteMediator.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using LiteMediator.Extensions;

namespace LiteMediator.Tests;

public class LiteMediatorTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;

    public LiteMediatorTests()
    {
        var services = new ServiceCollection();
        services.AddLiteMediator(options =>
        {
            options.Assemblies = new[] { typeof(TestRequestHandler).Assembly };
        });

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_ReturnsExpectedResponse()
    {
        var faker = new Faker();
        var request = new TestRequest(faker.Lorem.Sentence());

        var result = await _mediator.Send(request);

        Assert.StartsWith($"Hello:", result);
    }

    [Fact]
    public async Task Publish_CallsNotificationHandler()
    {
        var handler = Substitute.For<INotificationHandler<TestNotification>>();

        var services = new ServiceCollection();
        services.AddSingleton(handler);
        services.AddSingleton<IMediator>(sp => new Mediator(t => sp.GetService(t)));

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var notification = new TestNotification("hello");
        await mediator.Publish(notification);

        await handler.Received(1).Handle(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateStream_ReturnsExpectedValues()
    {
        var streamRequest = new TestStreamRequest(3);
        var values = new List<string>();

        await foreach (var value in _mediator.CreateStream(streamRequest))
        {
            values.Add(value);
        }

        Assert.Equal(new[] { "Item 1", "Item 2", "Item 3" }, values);
    }

}