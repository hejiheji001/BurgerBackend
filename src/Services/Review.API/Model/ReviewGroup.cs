namespace Review.API.Model;

public class ReviewGroup
{
    public int ListingItemId { get; set; }
    public List<ReviewItem> Items { get; set; }
    public double AverageScore { get; set; }
}