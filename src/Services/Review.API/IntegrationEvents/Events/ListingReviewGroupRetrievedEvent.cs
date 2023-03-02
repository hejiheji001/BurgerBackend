namespace Review.API.IntegrationEvents.Events;

public record ListingReviewGroupRetrievedEvent : IntegrationEvent
{
    public ListingReviewGroupRetrievedEvent(ReviewGroup reviewGroup, int listingItemId)
    {
        ReviewGroup = reviewGroup;
        ListingItemId = listingItemId;
    }

    public ReviewGroup ReviewGroup { get; }
    public int ListingItemId { get; }
}