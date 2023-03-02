namespace Review.API.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly ReviewContext _ReviewContext;
    private readonly IReviewIntegrationEventService _reviewIntegrationEventService;
    private readonly ReviewSettings _settings;
    private readonly IIdentityService _identityService;

    //IOptionsSnapshot provides scoped lifetime for the settings object
    //and ensures that the settings are reloaded if the underlying file changes
    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1#reload-configuration-data-with-ioptionssnapshot
    public ReviewController(ReviewContext context, IOptionsSnapshot<ReviewSettings> settings, IIdentityService identityService,
        IReviewIntegrationEventService reviewIntegrationEventService)
    {
        _ReviewContext = context ?? throw new ArgumentNullException(nameof(context));
        _reviewIntegrationEventService = reviewIntegrationEventService ??
                                         throw new ArgumentNullException(nameof(reviewIntegrationEventService));
        _settings = settings.Value;
        _identityService = identityService;
        
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    // GET api/v1/[controller]/items[?pageSize=3&pageIndex=10]
    [HttpGet]
    [Route("items")]
    [ProducesResponseType(typeof(IEnumerable<ReviewItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var itemsOnPage = await _ReviewContext.ReviewItems
            .OrderBy(c => c.CreationTime)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return Ok(itemsOnPage);
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
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult> CreateReviewItemAsync([FromBody] ReviewItem newReview)
    {
        var userId = _identityService.GetUserIdentity();
        Console.WriteLine(_identityService == null);
        
        if (userId == null)
        {
            return Forbid();
        }

        var item = new ReviewItem
        {
            Id = newReview.Id,
            ListingItemId = newReview.ListingItemId,
            TasteScore = newReview.TasteScore,
            TextureScore = newReview.TextureScore,
            VisualScore = newReview.VisualScore,
            Comment = newReview.Comment,
            CreationTime = DateTime.Now,
            ModificationTime = DateTime.Now,
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
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> UpdateReviewItemAsync([FromBody] ReviewItem newReview)
    {
        var userId = _identityService.GetUserIdentity();
        if (userId == null) return Forbid();
        
        //check if the item exists and belongs to thr userId
        var itemToUpdate = await _ReviewContext.ReviewItems.SingleOrDefaultAsync(ci => ci.Id == newReview.Id && ci.UserId == userId);
        
        //if not return no authorization
        if (itemToUpdate == null) return Forbid();
        
        itemToUpdate.ModificationTime = DateTime.Now;
        itemToUpdate.TasteScore = newReview.TasteScore;
        itemToUpdate.TextureScore = newReview.TextureScore;
        itemToUpdate.VisualScore = newReview.VisualScore;
        itemToUpdate.Comment = newReview.Comment;

        _ReviewContext.Update(itemToUpdate);
        var name = nameof(ItemsByIdAsync);
        return CreatedAtAction(name, new { id = newReview.Id }, null);
    }

    //DELETE api/v1/[controller]/id
    [Route("{id}")]
    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteReviewItemAsync(int id)
    {
        return NotFound("This API is Not Implemented.");
    }
}