namespace HowIdentity.Infrastructure.Processing.Consumer;

using MassTransit;

public class UserRegiserBulkResponseConsumerDefinition : ConsumerDefinition<UserRegiserBulkResponseConsumer>
{
    public UserRegiserBulkResponseConsumerDefinition()
    {
        ConcurrentMessageLimit = 1;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<UserRegiserBulkResponseConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(3)));
    }
}