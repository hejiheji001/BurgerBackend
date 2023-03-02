namespace Listing.API.IntegrationEvents;

public interface IListingIntegrationEventService
{
    Task SaveEventAsync(IntegrationEvent evt);
    Task PublishThroughEventBusAsync(IntegrationEvent evt);
}