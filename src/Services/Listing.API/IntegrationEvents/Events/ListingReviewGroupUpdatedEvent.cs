namespace Listing.API.IntegrationEvents.Events;

public record ListingReviewGroupUpdatedEvent : IntegrationEvent
{
    public ListingReviewGroupUpdatedEvent(int listingItemId)
    {
        ListingItemId = listingItemId;
    }
    public int ListingItemId { get; }
}