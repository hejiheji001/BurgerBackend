namespace Listing.API.IntegrationEvents.Events;

public record ListingVisitedEvent : IntegrationEvent
{
    public ListingVisitedEvent(int listingItemId)
    {
        ListingItemId = listingItemId;
    }

    public int ListingItemId { get; }
}