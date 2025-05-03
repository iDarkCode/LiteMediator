using LiteMediator.Abstractions;
using LiteMediator.Extensions;
using LiteMediator.Samples;
using LiteMediator.Samples.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging(config =>
{
    config.AddConsole();
});

services.AddLiteMediator(options =>
{
    options.Assemblies = [typeof(TestRequestHandler).Assembly];
    options.AddOpenBehavior(typeof(RequestLogginBehavior<,>));
    options.AddOpenBehavior(typeof(RequestLogginBehaviorDos<,>));
});



var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

var response = await mediator.Send(new TestRequest("Hello World"));
Console.WriteLine(response);

await mediator.Publish(new TestNotification("Test Event"));

await foreach (var item in mediator.CreateStream(new TestStreamRequest(5)))
{
    Console.WriteLine($"Stream item: {item}");
}
Console.ReadLine();
