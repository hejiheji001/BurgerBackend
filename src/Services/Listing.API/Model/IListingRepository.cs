namespace Listing.API.Model;

public interface IListingRepository
{
    Task<ListingGroup> GetListingGroupAsync(string searchId);
    IEnumerable<string> GetSearchIds();
    Task<ListingGroup> UpdateListingGroupAsync(ListingGroup listGroup);
    Task<bool> DeleteListingGroupAsync(string searchId);
}