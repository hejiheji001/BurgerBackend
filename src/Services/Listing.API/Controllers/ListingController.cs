using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace Listing.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ListingController : ControllerBase
{
    private readonly ListingContext _listingContext;
    private readonly IListingIntegrationEventService _listingIntegrationEventService;
    private readonly ListingSettings _settings;
    private readonly IRedisListingRepository _redisListingRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ListingController> _logger;

    //IOptionsSnapshot provides scoped lifetime for the settings object
    //and ensures that the settings are reloaded if the underlying file changes
    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1#reload-configuration-data-with-ioptionssnapshot
    public ListingController(ILogger<ListingController> logger, ListingContext context, IOptionsSnapshot<ListingSettings> settings,
        IListingIntegrationEventService listingIntegrationEventService,
        IRedisListingRepository redisListingRepository, IEventBus eventBus)
    {
        _listingContext = context ?? throw new ArgumentNullException(nameof(context));
        _listingIntegrationEventService = listingIntegrationEventService ??
                                          throw new ArgumentNullException(nameof(listingIntegrationEventService));
        _settings = settings.Value;
        _redisListingRepository = redisListingRepository;
        _logger = logger;
        _eventBus = eventBus;
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    // GET api/v1/[controller]/items[?pageSize=3&pageIndex=10]
    [HttpGet]
    [Route("items")]
    [ProducesResponseType(typeof(IEnumerable<ListingItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0,
        string? ids = null)
    {
        if (!string.IsNullOrEmpty(ids))
        {
            var items = await GetItemsByIdsAsync(ids);

            if (!items.Any()) return BadRequest("ids value invalid. Must be comma-separated list of numbers");

            return Ok(items);
        }

        var itemsOnPage = await _listingContext.ListingItems
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return Ok(itemsOnPage);
    }

    [HttpGet]
    [Route("items/{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ListingItem), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ListingItem>> ItemByIdAsync(int id)
    {
        if (id <= 0) return BadRequest();

        var item = await GetItemByIdAsync(id);

        if (item != null) return item;

        return NotFound();
    }

    private async Task<ActionResult<ListingItem>> GetItemByIdAsync(int id)
    {
        var itemGroup = await _redisListingRepository.GetListingGroupAsync(id.ToString());
        ListingItem result;
        if (itemGroup == null)
        {
            result = await _listingContext.ListingItems.SingleOrDefaultAsync(ci => ci.Id == id);
            if (result != null)
            {
                await _redisListingRepository.UpdateListingGroupAsync(new ListingGroup
                {
                    SearchId = id.ToString(),
                    Items = new List<ListingItem> { result }
                });
            }
            _logger.LogInformation("Listing item with id {Id} was not found in cache. Retrieved from database.", id);
        }
        else
        {
            result = itemGroup.Items.FirstOrDefault();
            _logger.LogInformation("Listing item with id {Id} was found in cache.", id);
        }

        var eventMessage = new ListingVisitedEvent(id);
        //Once a listing is visited, publish an to load reviews
        try
        {
            _logger.LogInformation("Listing item with id {Id} was visited.", id);
            _eventBus.Publish(eventMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName}", eventMessage.Id, Program.AppName);

            throw;
        }
            
        return result;
    }

    private async Task<List<ListingItem>> GetItemsByIdsAsync(string? ids)
    {
        var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out var x), Value: x));

        if (!numIds.All(nid => nid.Ok)) return new List<ListingItem>();

        var idsToSelect = numIds.Select(id => id.Value);
        var idsStr = string.Join("", idsToSelect.OrderDescending());
        
        List<ListingItem> result;
        var itemGroup = await _redisListingRepository.GetListingGroupAsync(idsStr);
        if (itemGroup == null)
        {
            result = await _listingContext.ListingItems.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();
            if (result.Any())
            {
                await _redisListingRepository.UpdateListingGroupAsync(new ListingGroup
                {
                    SearchId = idsStr,
                    Items = result
                });
            }
            _logger.LogInformation("Listing items with ids {Ids} were not found in cache. Retrieved from database.", ids);
        }
        else
        {
            result = itemGroup.Items;
            _logger.LogInformation("Listing items with ids {Ids} were found in cache.", ids);
        }
        
        return result;
    }
    
    private async Task<List<ListingItem>> GetItemsByNameAsync(string name, int pageSize, int pageIndex)
    {
        var searchId = $"{name}-{pageSize * pageIndex}";
        List<ListingItem> result;
        var itemGroup = await _redisListingRepository.GetListingGroupAsync(searchId);
        if (itemGroup == null)
        {
            result = await _listingContext.ListingItems
                .Where(c => c.Name.StartsWith(name))
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();
            
            if (result.Any())
            {
                await _redisListingRepository.UpdateListingGroupAsync(new ListingGroup
                {
                    SearchId = searchId,
                    Items = result
                });
            }
            _logger.LogInformation("Listing items with ids {Ids} were not found in cache. Retrieved from database.", searchId);
        }
        else
        {
            result = itemGroup.Items;
            _logger.LogInformation("Listing items with ids {Ids} were found in cache.", searchId);
        }
        
        return result;
    }
    
    // [HttpGet]
    // [Route("items/review/{id:int}")]
    // [ProducesResponseType((int)HttpStatusCode.NotFound)]
    // [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    // [ProducesResponseType(typeof(ListingReview), (int)HttpStatusCode.OK)]
    // public async Task<ActionResult<ListingGroup>> ItemReviewByIdAsync(int id)
    // {
    //     if (id <= 0) return BadRequest();
    //
    //     var item = await _listingContext.ListingItems.SingleOrDefaultAsync(ci => ci.Id == id);
    //
    //     if (item != null)
    //     {
    //         var listingReview = _reviewService.GetListingReview(item.Id);
    //         listingReview.AverageScore = listingReview.ReviewItems.Any()
    //             ? listingReview.ReviewItems.Average(s => (s.TasteScore + s.TextureScore + s.VisualScore) / 3)
    //             : 0;
    //         return Ok(listingReview);
    //     }
    //
    //     return NotFound();
    // }

    // GET api/v1/[controller]/items/withname/[?longitude=3&latitude=10&withinMeters=100]
    [HttpGet]
    [Route("items/withlocation")]
    [ProducesResponseType(typeof(IEnumerable<ListingItem>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ItemsWithLocationAsync([FromQuery] double longitude = 114.017211,
        [FromQuery] double latitude = 22.637782, [FromQuery] int range = 100)
    {
        var point = ProjectTo(new Point(longitude, latitude));

        //https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8
        var itemsOnPage = await _listingContext.ListingItems
            .AsAsyncEnumerable()
            .Where(item =>
            {
                //Multiply the degrees of separation of longitude and latitude by 111,139 to get the corresponding linear distances in meters
                //https://sciencing.com/convert-latitude-longtitude-feet-2724.html
                item.Distance = point.Distance(ProjectTo(item.Location)) * 111139;
                return item.Distance <= range;
            }).ToListAsync();
        return Ok(itemsOnPage);
    }

    Geometry ProjectTo(Geometry geometry)
    {
        //https://learn.microsoft.com/en-us/ef/core/modeling/spatial#srid-ignored-during-client-operations
        var coordinateSystemServices = new CoordinateSystemServices(new Dictionary<int, string>
        {
            [4326] = GeographicCoordinateSystem.WGS84.WKT,

            //https://epsg.io/4480 China Geodetic Coordinate System
            [4480] = @"GEOGCS[""China_Geodetic_Coordinate_System_2000_3D"",
    DATUM[""D_China_2000"",
        SPHEROID[""CGCS2000"",6378137.0,298.257222101]],
    PRIMEM[""Greenwich"",0.0],
    UNIT[""Degree"",0.0174532925199433],
    LINUNIT[""Meter"",1.0]]"
        });

        var transform =
            new MathTransformFilter(coordinateSystemServices.CreateTransformation(4480, 4326).MathTransform);
        geometry.Apply(transform);

        return geometry;
    }

    private class MathTransformFilter : ICoordinateSequenceFilter
    {
        private readonly MathTransform _transform;

        public MathTransformFilter(MathTransform transform)
            => _transform = transform;

        public bool Done => false;
        public bool GeometryChanged => true;

        public void Filter(CoordinateSequence seq, int i)
        {
            var x = seq.GetX(i);
            var y = seq.GetY(i);
            var z = seq.GetZ(i);
            _transform.Transform(ref x, ref y, ref z);
            seq.SetX(i, x);
            seq.SetY(i, y);
            seq.SetZ(i, z);
        }
    }

    // GET api/v1/[controller]/items/withname/samplename[?pageSize=3&pageIndex=10]
    [HttpGet]
    [Route("items/withname/{name:minlength(1)}")]
    [ProducesResponseType(typeof(IEnumerable<ListingItem>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ItemsWithNameAsync(string name, [FromQuery] int pageSize = 10,
        [FromQuery] int pageIndex = 0)
    {
        var itemsOnPage =  await GetItemsByNameAsync(name, pageSize, pageIndex);
        return Ok(itemsOnPage);
    }

    //POST api/v1/[controller]/items
    [Route("items")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> CreateListingItemAsync([FromBody] ListingItem item)
    {
        return NotFound("This API is Not Implemented.");
    }

    //PUT api/v1/[controller]/items
    [Route("items")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> UpdateListingItemAsync([FromBody] ListingItem item)
    {
        return NotFound("This API is Not Implemented.");
    }

    //DELETE api/v1/[controller]/id
    [Route("{id}")]
    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteListingItemAsync(int id)
    {
        return NotFound("This API is Not Implemented.");
    }
}