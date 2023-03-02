namespace Listing.API.Events;

public interface IListingIntegrationEventService
{
    Task SaveEventAsync(IntegrationEvent evt);
    Task PublishThroughEventBusAsync(IntegrationEvent evt);
}
