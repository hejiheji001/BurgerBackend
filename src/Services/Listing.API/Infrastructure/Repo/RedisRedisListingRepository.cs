using System.Diagnostics;
using NetTopologySuite.IO.Converters;

namespace Listing.API.Infrastructure.Repo;

public class RedisRedisListingRepository : IRedisListingRepository
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisRedisListingRepository> _logger;
    private readonly ConnectionMultiplexer _redis;

    public RedisRedisListingRepository(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
    {
        _logger = loggerFactory.CreateLogger<RedisRedisListingRepository>();
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<bool> DeleteListingGroupAsync(string searchId)
    {
        return await _database.KeyDeleteAsync(searchId);
    }

    public async Task<bool> UpdateListingReviewGroupAsync(int searchId, ReviewGroup eventReviewGroup)
    {
        var data = await _database.StringGetAsync(searchId.ToString());

        if (data.IsNullOrEmpty) return false;

        var listingGroup = JsonSerializer.Deserialize<ListingGroup>(data, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new GeoJsonConverterFactory() }
        });

        if (listingGroup != null)
        {
            listingGroup.Items.First().ReviewGroup = eventReviewGroup;
            await UpdateListingGroupAsync(listingGroup);
            return true;
        }

        return false;
    }

    public IEnumerable<string> GetSearchIds()
    {
        var server = GetServer();
        var data = server.Keys();

        return data?.Select(k => k.ToString());
    }

    public async Task<ListingGroup> GetListingGroupAsync(string searchId)
    {
        var data = await _database.StringGetAsync(searchId);

        if (data.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<ListingGroup>(data, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new GeoJsonConverterFactory() }
        });
    }

    public async Task<ListingGroup> UpdateListingGroupAsync(ListingGroup listGroup)
    {
        var searchId = listGroup.SearchId;
        var created = await _database.StringSetAsync(searchId, JsonSerializer.Serialize(listGroup,
            new JsonSerializerOptions
            {
                Converters = { new GeoJsonConverterFactory() }
            }));

        if (!created)
        {
            _logger.LogInformation("Problem occur persisting the item.");
            return null;
        }

        _logger.LogInformation("ListingGroup item persisted succesfully.");

        return await GetListingGroupAsync(searchId);
    }

    private IServer GetServer()
    {
        var endpoint = _redis.GetEndPoints();
        return _redis.GetServer(endpoint.First());
    }
}