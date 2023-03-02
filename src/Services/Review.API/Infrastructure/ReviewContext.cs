using Review.API.Model;

namespace Review.API.Infrastructure;

public class ReviewContext : DbContext
{
    public ReviewContext(DbContextOptions<ReviewContext> options) : base(options)
    {
    }

    public DbSet<ReviewItem> ReviewItems { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        var reviews = builder.Entity<ReviewItem>();
        reviews.ToTable("Review");
        reviews.Property(ci => ci.Id).IsRequired();
        reviews.Property(ci => ci.UserId).HasMaxLength(36).IsRequired();
        reviews.Property(ci => ci.ListingItemId).IsRequired();
        reviews.Property(ci => ci.TextureScore).IsRequired();
        reviews.Property(ci => ci.TextureScore).IsRequired();
        reviews.Property(ci => ci.VisualScore).IsRequired();
        reviews.Property(ci => ci.Comment).IsRequired().HasMaxLength(200);
        reviews.Property(ci => ci.CreationTime).IsRequired();
        reviews.Property(ci => ci.ModificationTime).IsRequired();
        reviews.Property(ci => ci.ImageUrl).IsRequired(false);
    }
}