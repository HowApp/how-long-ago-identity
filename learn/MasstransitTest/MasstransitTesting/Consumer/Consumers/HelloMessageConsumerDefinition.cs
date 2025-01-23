namespace Consumer.Consumers;

using MassTransit;

public class HelloMessageConsumerDefinition : ConsumerDefinition<HelloMessageConsumer>
{
    public HelloMessageConsumerDefinition()
    {
        // do not need this if configure in UsingRabbitMq ReceiveEndpoint queue name
        // EndpointName = "sample-queue";
        // EndpointName = KebabCaseEndpointNameFormatter.Instance.Consumer<HelloMessageConsumer>();
        ConcurrentMessageLimit = 4;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<HelloMessageConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // configuration in this definition can be done in ReceiveEndpoint for all queue
        // now we can set configuration only for this Consumer
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));

        endpointConfigurator.UseInMemoryOutbox(context);
    }
}