namespace Image.API.IntegrationEvents;

public interface IImageIntegrationEventService
{
    Task SaveEventAsync(IntegrationEvent evt);
    Task PublishThroughEventBusAsync(IntegrationEvent evt);
}