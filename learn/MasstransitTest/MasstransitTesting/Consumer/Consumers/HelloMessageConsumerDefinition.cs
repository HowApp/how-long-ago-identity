namespace Consumer.Consumers;

using MassTransit;

public class HelloMessageConsumerDefinition : ConsumerDefinition<HelloMessageConsumer>
{
    public HelloMessageConsumerDefinition()
    {
        // EndpointName = "hello-queue";
        ConcurrentMessageLimit = 4;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<HelloMessageConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));

        endpointConfigurator.UseInMemoryOutbox(context);
    }
}