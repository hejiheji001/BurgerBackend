namespace Listing.API.Infrastructure.Repo;

public interface IRedisListingRepository
{
    Task<ListingGroup> GetListingGroupAsync(string searchId);
    IEnumerable<string> GetSearchIds();
    Task<ListingGroup> UpdateListingGroupAsync(ListingGroup listGroup);
    Task<bool> DeleteListingGroupAsync(string searchId);
    Task<bool> UpdateListingReviewGroupAsync(int eventListingItemId, ReviewGroup eventReviewGroup);
}