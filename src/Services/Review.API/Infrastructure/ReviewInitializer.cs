using System.Drawing;

namespace Review.API.Infrastructure;

public class ListingInitializer
{
    public async Task SeedAsync(ListingContext context, ILogger<ListingInitializer> logger)
    {
        var policy = CreatePolicy(logger, nameof(ListingInitializer));
        
        await policy.ExecuteAsync(async () =>
        {
            Console.WriteLine(context.ListingItems.Any());
            if (!context.ListingItems.Any())
            {
                await context.ListingItems.AddRangeAsync(GetPreconfiguredItems());

                await context.SaveChangesAsync();
            }
        });
    }

    private IEnumerable<ListingItem> GetPreconfiguredItems()
    {
        //Coordinates in NTS are in terms of X and Y values. To represent longitude and latitude,
        //use X for longitude and Y for latitude.
        //Note that this is backwards from the latitude, longitude format in which you typically see these values.
        //https://learn.microsoft.com/en-us/ef/core/modeling/spatial

        //Some points for KFC & MCD from map.baidu.com
        return new List<ListingItem>()
        {
            new()
            {
                Name = "麦当劳(淮海百盛店)", Description = "淮海中路918号百盛购物中心B1", Location = new Point(121.46615, 31.22361),
                OpeningTimeStart = new TimeOnly(7, 0), OpeningTimeEnd = new TimeOnly(22, 0)
            },
            new()
            {
                Name = "肯德基(永隆餐厅)", Description = "淮海中路1298号", Location = new Point(121.457175, 31.219813),
                OpeningTimeStart = new TimeOnly(6, 0), OpeningTimeEnd = new TimeOnly(23, 0)
            },
            new()
            {
                Name = "肯德基(金钟店)", Description = "淮海中路98号金钟广场1楼", Location = new Point(121.485304, 31.231457),
                OpeningTimeStart = new TimeOnly(6, 0), OpeningTimeEnd = new TimeOnly(02, 0)
            },
            new()
            {
                Name = "肯德基(上塘荟餐厅)", Description = "民塘路521号", Location = new Point(114.020779, 22.642474),
                OpeningTimeStart = new TimeOnly(0, 0), OpeningTimeEnd = new TimeOnly(23, 59, 59)
            },
            new()
            {
                Name = "肯德基(深圳北站店)", Description = "致远中路28号深圳北站F2", Location = new Point(114.035886, 22.616418),
                OpeningTimeStart = new TimeOnly(6, 0), OpeningTimeEnd = new TimeOnly(22, 0)
            },
            new()
            {
                Name = "麦当劳(龙胜店)", Description = "和平里花园1期商业一半地下17、18、19,商业一02层",
                Location = new Point(114.017964, 22.649974),
                OpeningTimeStart = new TimeOnly(0, 0), OpeningTimeEnd = new TimeOnly(23, 59, 59)
            }
        };
    }

    private AsyncRetryPolicy CreatePolicy(ILogger<ListingInitializer> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<SqlException>().WaitAndRetryAsync(
            retryCount: retries,
            sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
            onRetry: (exception, timeSpan, retry, ctx) =>
            {
                logger.LogWarning(exception,
                    "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}",
                    prefix, exception.GetType().Name, exception.Message, retry, retries);
            }
        );
    }
}