using System.Security.Claims;
using Review.API.Model;
using Review.API.Services;

namespace Review.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly ReviewContext _ReviewContext;
    private readonly IReviewIntegrationEventService _ReviewIntegrationEventService;
    private readonly ReviewSettings _settings;
    private readonly IIdentityService _identityService;

    //IOptionsSnapshot provides scoped lifetime for the settings object
    //and ensures that the settings are reloaded if the underlying file changes
    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1#reload-configuration-data-with-ioptionssnapshot
    public ReviewController(ReviewContext context, IOptionsSnapshot<ReviewSettings> settings, IIdentityService identityService,
        IReviewIntegrationEventService ReviewIntegrationEventService)
    {
        _ReviewContext = context ?? throw new ArgumentNullException(nameof(context));
        _ReviewIntegrationEventService = ReviewIntegrationEventService ??
                                         throw new ArgumentNullException(nameof(ReviewIntegrationEventService));
        _settings = settings.Value;
        _identityService = identityService;
        
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    // GET api/v1/[controller]/items[?pageSize=3&pageIndex=10]
    [HttpGet]
    [Route("items")]
    [ProducesResponseType(typeof(IEnumerable<ReviewItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0,
        string? ids = null)
    {
        var itemsOnPage = await _ReviewContext.ReviewItems
            .OrderBy(c => c.CreationTime)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return Ok(itemsOnPage);
    }

    private async Task<List<ReviewItem>> GetItemsByIdsAsync(string? ids)
    {
        var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out var x), Value: x));

        if (!numIds.All(nid => nid.Ok)) return new List<ReviewItem>();

        var idsToSelect = numIds.Select(id => id.Value);

        var items = await _ReviewContext.ReviewItems.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();

        return items;
    }

    [HttpGet]
    [Route("items/{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<ReviewItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ReviewItem>> ItemsByIdAsync(int id)
    {
        if (id <= 0) return BadRequest();

        var item = await _ReviewContext.ReviewItems.SingleOrDefaultAsync(ci => ci.Id == id);

        if (item != null) return item;

        return NotFound();
    }

    //POST api/v1/[controller]/items
    [Route("items")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> CreateReviewItemAsync([FromBody] ReviewItem newReview)
    {
        var userId = _identityService.GetUserIdentity();
        var userName = this.HttpContext.User.FindFirst(x => x.Type == ClaimTypes.Name).Value;
        
        var item = new ReviewItem
        {
            Id = newReview.Id,
            ListingItemId = newReview.ListingItemId,
            TasteScore = newReview.TasteScore,
            TextureScore = newReview.TextureScore,
            VisualScore = newReview.VisualScore,
            CreationTime = DateTime.UtcNow,
            Comment = newReview.Comment,
            UserId = userId
        };

        _ReviewContext.ReviewItems.Add(item);

        await _ReviewContext.SaveChangesAsync();

        var name = nameof(ItemsByIdAsync);
        return CreatedAtAction(name, new { id = item.Id }, null);
    }

    //PUT api/v1/[controller]/items
    [Route("items")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> UpdateReviewItemAsync([FromBody] ReviewItem item)
    {
        throw new NotImplementedException();
    }

    //DELETE api/v1/[controller]/id
    [Route("{id}")]
    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteReviewItemAsync(int id)
    {
        throw new NotImplementedException();
    }
}