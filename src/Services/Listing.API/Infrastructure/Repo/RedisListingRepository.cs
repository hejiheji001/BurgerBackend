namespace Listing.API.Infrastructure.Repo;

public class RedisListingRepository : IListingRepository
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisListingRepository> _logger;
    private readonly ConnectionMultiplexer _redis;

    public RedisListingRepository(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
    {
        _logger = loggerFactory.CreateLogger<RedisListingRepository>();
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<bool> DeleteListingGroupAsync(string searchId)
    {
        return await _database.KeyDeleteAsync(searchId);
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
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<ListingGroup> UpdateListingGroupAsync(ListingGroup listGroup)
    {
        var searchId = listGroup.SearchId.ToString();
        var created = await _database.StringSetAsync(searchId, JsonSerializer.Serialize(listGroup));

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