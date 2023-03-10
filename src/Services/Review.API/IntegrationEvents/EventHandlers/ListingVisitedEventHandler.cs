using Review.API.Infrastructure.Repo;
using ListingVisitedEvent = Review.API.IntegrationEvents.Events.ListingVisitedEvent;

namespace Review.API.IntegrationEvents.EventHandlers;

public class ListingVisitedEventHandler : IIntegrationEventHandler<ListingVisitedEvent>
{
    private readonly IEventBus _eventBus;
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<ListingVisitedEventHandler> _logger;

    public ListingVisitedEventHandler(IEventBus eventBus, IReviewRepository reviewRepository, ILogger<ListingVisitedEventHandler> logger)
    {
        _eventBus = eventBus;
        _reviewRepository = reviewRepository;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ListingVisitedEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation(
                "----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);
            var reviewGroup = await _reviewRepository.GetReviewGroup(@event.ListingItemId);
            var eventMessage = new ListingReviewGroupRetrievedEvent(reviewGroup, @event.ListingItemId);
            _eventBus.Publish(eventMessage);
        }
    }
}