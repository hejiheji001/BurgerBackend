namespace Listing.API.Infrastructure;

public class ListingContext : DbContext
{
    public ListingContext(DbContextOptions<ListingContext> options) : base(options)
    {
    }

    public DbSet<ListingItem> ListingItems { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        var listing = builder.Entity<ListingItem>();
        listing.ToTable("Listing");
        listing.Property(ci => ci.Id).IsRequired();
        listing.Property(ci => ci.Name).IsRequired().HasMaxLength(50);
        listing.Property(ci => ci.Description).IsRequired().HasMaxLength(200);

        //By default, spatial properties are mapped to geography columns in SQL Server.
        //To use geometry, configure the column type in your model.
        //https://learn.microsoft.com/en-us/ef/core/providers/sql-server/spatial
        listing.Property(ci => ci.Location).HasColumnType("geometry").IsRequired();
        listing.Property(ci => ci.OpeningTimeStart).IsRequired();
        listing.Property(ci => ci.OpeningTimeEnd).IsRequired();
        listing.Property(ci => ci.ImageUrl).IsRequired(false);
        listing.Ignore(ci => ci.IsOpen);
        listing.Ignore(ci => ci.Distance);
    }
}