namespace Listing.API.Model;

public record ListingItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Geometry Location { get; set; }
    public TimeOnly OpeningTimeStart { get; set; }
    public TimeOnly OpeningTimeEnd { get; set; }
    public bool IsOpen => TimeOnly.FromDateTime(DateTime.Now).IsBetween(OpeningTimeStart, OpeningTimeEnd);
    public string ImageUrl { get; set; }
}