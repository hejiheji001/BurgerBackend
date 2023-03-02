namespace Listing.API.IntegrationEvents.EventHandlers;

public class PlaceReviewUpdatedEventHandler : IIntegrationEventHandler<PlaceReviewUpdatedEvent>
{
    private readonly ListingContext _listingContext;
    private readonly ILogger<PlaceReviewUpdatedEventHandler> _logger;

    public PlaceReviewUpdatedEventHandler(
        ListingContext listingContext,
        ILogger<PlaceReviewUpdatedEventHandler> logger)
    {
        _listingContext = listingContext;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(PlaceReviewUpdatedEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation(
                "----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);
            await _listingContext.SaveChangesAsync();
        }
    }
}