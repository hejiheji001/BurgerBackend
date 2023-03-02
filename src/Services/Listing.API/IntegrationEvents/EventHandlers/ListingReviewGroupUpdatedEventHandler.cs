namespace Listing.API.IntegrationEvents.EventHandlers;

public class ListingReviewGroupUpdatedEventHandler : IIntegrationEventHandler<ListingReviewGroupUpdatedEvent>
{
    private readonly ILogger<ListingReviewGroupRetrievedEventHandler> _logger;

    public ListingReviewGroupUpdatedEventHandler(ILogger<ListingReviewGroupRetrievedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ListingReviewGroupUpdatedEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation(
                "----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);
            // Do some UI update
        }
    }
}
