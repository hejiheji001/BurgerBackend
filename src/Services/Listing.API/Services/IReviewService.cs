namespace Listing.API.Services;

public interface IReviewService
{
    public ListingReview GetListingReview(int listingId);
}