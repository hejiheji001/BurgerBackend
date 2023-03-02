namespace Listing.API.IntegrationEvents.Events;

public record PlaceReviewUpdatedEvent : IntegrationEvent
{
    public PlaceReviewUpdatedEvent(int listingItemId)
    {
        ListingItemId = listingItemId;
    }

    public int ListingItemId { get; }
}