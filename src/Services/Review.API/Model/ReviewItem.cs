namespace Review.API.Model;

public record ReviewItem
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int ListingItemId { get; set; }
    public int TasteScore { get; set; }
    public int TextureScore { get; set; }
    public int VisualScore { get; set; }

    // Each Listing can have multiple reviews, so we need to calculate the average rate
    public double AverageScore { get; set; }
    public string Comment { get; set; }

    public DateTime CreationTime
    {
        get => DateTime.Now;
        set => CreationTime = value;
    }

    public DateTime ModificationTime { get; set; }
    public string ImageUrl { get; set; }
}