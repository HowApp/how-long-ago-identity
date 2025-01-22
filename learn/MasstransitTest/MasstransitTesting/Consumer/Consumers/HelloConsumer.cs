namespace Consumer.Consumers;

using Contract;
using MassTransit;

public class HelloConsumer : IConsumer<HelloMessage>
{
    private readonly ILogger<HelloConsumer> _logger;

    public HelloConsumer(ILogger<HelloConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<HelloMessage> context)
    {
        _logger.LogInformation($"Received message: {context.Message}");
        
        return Task.CompletedTask;
    }
}