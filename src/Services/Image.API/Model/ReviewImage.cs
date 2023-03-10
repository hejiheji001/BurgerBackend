namespace Image.API.Model;

public record ReviewImage : Image
{
    public int ReviewId { get; set; }
}