namespace Consumer.Consumers;

using Contract;
using MassTransit;

public class HelloMessageConsumer : IConsumer<HelloMessage>
{
    private readonly ILogger<HelloMessageConsumer> _logger;

    public HelloMessageConsumer(ILogger<HelloMessageConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<HelloMessage> context)
    {
        _logger.LogInformation($"Received message: {context.Message.Message}");
        
        return Task.CompletedTask;
    }
}