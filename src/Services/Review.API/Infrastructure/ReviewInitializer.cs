namespace Review.API.Infrastructure;

public class ReviewInitializer
{
    
    public async Task SeedAsync(ReviewContext context, ILogger<ReviewInitializer> logger)
    {
        var policy = CreatePolicy(logger, nameof(ReviewInitializer));
        
        await policy.ExecuteAsync(async () =>
        {
            if (!context.ReviewItems.Any())
            {
                await context.ReviewItems.AddRangeAsync(GetPreconfiguredItems());

                await context.SaveChangesAsync();
            }
        });
    }

    private IEnumerable<Model.Review> GetPreconfiguredItems()
    {
        var alice = "1";
        var bob = "2";
        
        return new List<Model.Review>
        {
            new()
            {
                ListingItemId = 1, UserId = alice, TasteScore = 5, TextureScore = 5, VisualScore = 5,
                ModificationTime = DateTime.Now, Comment = "Fantastic place to eat"
            },
            new()
            {
                ListingItemId = 1, UserId = bob, TasteScore = 4, TextureScore = 4, VisualScore = 4,
                ModificationTime = DateTime.Now, Comment = "Great place to eat"
            },
            new()
            {
                ListingItemId = 2, UserId = alice, TasteScore = 3, TextureScore = 3, VisualScore = 3,
                ModificationTime = DateTime.Now, Comment = "Good place to eat"
            },
            new()
            {
                ListingItemId = 2, UserId = bob, TasteScore = 4, TextureScore = 4, VisualScore = 4,
                ModificationTime = DateTime.Now, Comment = "Great place to eat"
            },
            new()
            {
                ListingItemId = 3, UserId = alice, TasteScore = 2, TextureScore = 2, VisualScore = 2,
                ModificationTime = DateTime.Now, Comment = "Normal place to eat"
            },
            new()
            {
                ListingItemId = 3, UserId = bob, TasteScore = 1, TextureScore = 1, VisualScore = 1,
                ModificationTime = DateTime.Now, Comment = "Terrible place to eat"
            },
            new()
            {
                ListingItemId = 4, UserId = alice, TasteScore = 5, TextureScore = 5, VisualScore = 5,
                ModificationTime = DateTime.Now, Comment = "Fantastic place to eat"
            },
            new()
            {
                ListingItemId = 4, UserId = bob, TasteScore = 4, TextureScore = 4, VisualScore = 4,
                ModificationTime = DateTime.Now, Comment = "Great place to eat"
            },
            new()
            {
                ListingItemId = 5, UserId = alice, TasteScore = 2, TextureScore = 2, VisualScore = 2,
                ModificationTime = DateTime.Now, Comment = "Normal place to eat"
            },
            new()
            {
                ListingItemId = 5, UserId = bob, TasteScore = 1, TextureScore = 1, VisualScore = 1,
                ModificationTime = DateTime.Now, Comment = "Terrible place to eat"
            },
            new()
            {
                ListingItemId = 6, UserId = alice, TasteScore = 5, TextureScore = 5, VisualScore = 5,
                ModificationTime = DateTime.Now, Comment = "Fantastic place to eat"
            },
            new()
            {
                ListingItemId = 6, UserId = bob, TasteScore = 4, TextureScore = 4, VisualScore = 4,
                ModificationTime = DateTime.Now, Comment = "Great place to eat"
            }
        };
    }

    private AsyncRetryPolicy CreatePolicy(ILogger<ReviewInitializer> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<SqlException>().WaitAndRetryAsync(
            retries,
            retry => TimeSpan.FromSeconds(5),
            (exception, timeSpan, retry, ctx) =>
            {
                logger.LogWarning(exception,
                    "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}",
                    prefix, exception.GetType().Name, exception.Message, retry, retries);
            }
        );
    }

}