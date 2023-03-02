namespace Review.API.Infrastructure.Repo;

public class ReviewRepository : IReviewRepository
{
    private readonly ReviewContext _reviewContext;
    
    public ReviewRepository(ReviewContext context)
    {
        _reviewContext = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<ReviewGroup> GetReviewGroup(int listingId)
    {
        if (listingId <= 0) return null;

        var items = await _reviewContext.ReviewItems.Where(ci => ci.ListingItemId == listingId).ToListAsync();

        if (items.Any())
        {
            var reviewGroup = new ReviewGroup
            {
                ListingItemId = listingId,
                Items = items,
                AverageScore = items.Average(i => (i.TasteScore + i.TextureScore + i.VisualScore) / 3 )
            };
            return reviewGroup;
        }
        return null;
    }
}