namespace Listing.API.IntegrationEvents.EventHandlers;

public class ListingReviewGroupRetrievedEventHandler : IIntegrationEventHandler<ListingReviewGroupRetrievedEvent>
{
    private readonly IRedisListingRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ListingReviewGroupRetrievedEventHandler> _logger;

    public ListingReviewGroupRetrievedEventHandler(IEventBus eventBus, IRedisListingRepository repository, ILogger<ListingReviewGroupRetrievedEventHandler> logger)
    {
        _eventBus = eventBus;
        _repository = repository;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ListingReviewGroupRetrievedEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation(
                "----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);
            // var eventMessage = new ListingReviewGroupRetrievedEvent(reviewGroup, @event.ListingItemId);
            // _eventBus.Publish(eventMessage);
            await _repository.UpdateListingReviewGroupAsync(@event.ListingItemId, @event.ReviewGroup);
            
            _eventBus.Publish(new ListingReviewGroupUpdatedEvent(@event.ListingItemId));
        }
    }
}
