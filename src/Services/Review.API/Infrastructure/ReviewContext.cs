namespace Review.API.Infrastructure;

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
    }

    //https://github.com/dotnet/efcore/issues/18058
    //https://github.com/NetTopologySuite/NetTopologySuite/issues/349
    //https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(
            "Server=tcp:127.0.0.1,6433;Initial Catalog=BurgerBackend.ListingDb;User Id=sa;Password=Pass@word;Encrypt=False;",
            x => x.UseNetTopologySuite());
    }
}
