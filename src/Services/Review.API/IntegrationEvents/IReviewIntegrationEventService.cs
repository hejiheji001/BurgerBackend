namespace Review.API.IntegrationEvents;

public interface IReviewIntegrationEventService
{
    Task SaveEventAsync(IntegrationEvent evt);
    Task PublishThroughEventBusAsync(IntegrationEvent evt);
}