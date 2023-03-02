using EventBus.Events;

namespace Review.API.IntegrationEvents.Events;

public record ReviewUpdateEvent : IntegrationEvent
{
    public int ListingItemId { get; }

    public ReviewUpdateEvent(int listingItemId)
    {
        ListingItemId = listingItemId;
    }
}
