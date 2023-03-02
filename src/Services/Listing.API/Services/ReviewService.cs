namespace Listing.API.Services;

public class ReviewService : IReviewService
{
    public ListingReview GetListingReview(int listingId)
    {
        Console.WriteLine("Should Interact with Review.API");
        return new ListingReview();
    }
}