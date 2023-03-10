namespace Image.API.Infrastructure;

public class ImageContext : DbContext
{
    public ImageContext(DbContextOptions<ImageContext> options) : base(options)
    {
    }

    public DbSet<ListingImage> ListingImages { get; set; }
    public DbSet<ReviewImage> ReviewImages { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        var listingImage = builder.Entity<ListingImage>();
        listingImage.ToTable("ListingImage");
        listingImage.Property(ci => ci.Id).IsRequired();
        listingImage.Property(ci => ci.ListingItemId).IsRequired();
        listingImage.Property(ci => ci.Url).IsRequired();
        
        var reviewImage = builder.Entity<ReviewImage>();
        reviewImage.ToTable("ListingImage");
        reviewImage.Property(ci => ci.Id).IsRequired();
        reviewImage.Property(ci => ci.ReviewId).IsRequired();
        reviewImage.Property(ci => ci.Url).IsRequired();
    }
}