
namespace Listing.API.Infra;

public class ListingContext : DbContext
{
    public ListingContext(DbContextOptions<ListingContext> options) : base(options)
    {
    }
    
    public DbSet<ListingItem> ListingItems { get; set; }
}


public class ListingContextDesignFactory : IDesignTimeDbContextFactory<ListingContext>
{
    public ListingContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ListingContext>()
            .UseSqlServer("Server=.;Initial Catalog=Microsoft.eShopOnContainers.Services.CatalogDb;Integrated Security=true");

        return new ListingContext(optionsBuilder.Options);
    }
}
