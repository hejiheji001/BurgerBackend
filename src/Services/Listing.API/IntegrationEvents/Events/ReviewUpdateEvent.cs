namespace Listing.API.IntegrationEvents.Events;

public record ReviewUpdateEvent : IntegrationEvent
{
    public ReviewUpdateEvent(int listingItemId)
    {
        ListingItemId = listingItemId;
    }

    public int ListingItemId { get; }
}