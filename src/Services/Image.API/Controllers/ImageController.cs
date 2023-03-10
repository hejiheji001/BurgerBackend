using Image.API.Utilities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Image.API.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class ImageController : ControllerBase
{
    private readonly ImageContext _imageContext;
    private readonly IImageIntegrationEventService _imageIntegrationEventService;
    private readonly ImageSettings _settings;
    private readonly IIdentityService _identityService;
    private readonly IImageService _imageService;
    private static readonly FormOptions _defaultFormOptions = new FormOptions();

    //IOptionsSnapshot provides scoped lifetime for the settings object
    //and ensures that the settings are reloaded if the underlying file changes
    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1#reload-configuration-data-with-ioptionssnapshot
    public ImageController(ImageContext context, IOptionsSnapshot<ImageSettings> settings,
        IIdentityService identityService,
        IImageIntegrationEventService imageIntegrationEventService, IImageService imageService)
    {
        _imageContext = context ?? throw new ArgumentNullException(nameof(context));
        _imageIntegrationEventService = imageIntegrationEventService ??
                                        throw new ArgumentNullException(nameof(imageIntegrationEventService));
        _settings = settings.Value;
        _identityService = identityService;
        _imageService = imageService;

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    //upload an image from post request, using file stream
    [HttpPost]
    [Route("review/{id:int}/upload")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> Post(int reviewId)
    {
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            // Log error
            return BadRequest(ModelState);
        }

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType), 1000);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
        _imageService.ReviewImageUpload(reader, reviewId);

        return Created("", null);
    }
}