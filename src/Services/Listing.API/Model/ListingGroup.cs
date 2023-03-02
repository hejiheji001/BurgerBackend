namespace Listing.API.Model;

public class ListingGroup
{
    public Guid SearchId { get; set; }
    public List<ListingItem> Items { get; set; }
}