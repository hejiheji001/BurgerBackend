namespace Listing.API.Model;

public class ListingReview
{
    public int Id { get; set; }
    public List<Reviews> ReviewItems { get; set; }
    public double AverageScore { get; set; }
}