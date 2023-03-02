namespace Listing.API.IntegrationEvents.Events;

public record PlaceStatusChangedToOpenEvent : IntegrationEvent
{
    public PlaceStatusChangedToOpenEvent(int listingItemId)
    {
        ListingItemId = listingItemId;
    }

    public int ListingItemId { get; }
}