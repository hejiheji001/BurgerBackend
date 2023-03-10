namespace Image.API.Model;

public record ListingImage : Image
{
    public int ListingItemId { get; set; }
}