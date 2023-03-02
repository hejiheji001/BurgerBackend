namespace Review.API.IntegrationEvents.EventHandlers;

public class PlaceReviewUpdatedEventHandler : IIntegrationEventHandler<PlaceReviewUpdatedEvent>
{
    private readonly ILogger<PlaceReviewUpdatedEventHandler> _logger;
    private readonly ReviewContext _reviewContext;

    public PlaceReviewUpdatedEventHandler(
        ReviewContext ReviewContext,
        ILogger<PlaceReviewUpdatedEventHandler> logger)
    {
        _reviewContext = ReviewContext;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(PlaceReviewUpdatedEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation(
                "----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);
            await _reviewContext.SaveChangesAsync();
        }
    }
}