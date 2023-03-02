namespace Listing.API.IntegrationEvents.EventHandlers;

public class PlaceStatusChangedToOpenEventHandler : IIntegrationEventHandler<PlaceStatusChangedToOpenEvent>
{
    private readonly ListingContext _listingContext;
    private readonly ILogger<PlaceStatusChangedToOpenEventHandler> _logger;

    public PlaceStatusChangedToOpenEventHandler(
        ListingContext listingContext,
        ILogger<PlaceStatusChangedToOpenEventHandler> logger)
    {
        _listingContext = listingContext;
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    public async Task Handle(PlaceStatusChangedToOpenEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);
            await _listingContext.SaveChangesAsync();
        }
    }
}
