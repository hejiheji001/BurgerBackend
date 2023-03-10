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

    //IOptionsSnapshot provides scoped lifetime for the settings object
    //and ensures that the settings are reloaded if the underlying file changes
    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1#reload-configuration-data-with-ioptionssnapshot
    public ImageController(ImageContext context, IOptionsSnapshot<ImageSettings> settings, IIdentityService identityService,
        IImageIntegrationEventService imageIntegrationEventService)
    {
        _imageContext = context ?? throw new ArgumentNullException(nameof(context));
        _imageIntegrationEventService = imageIntegrationEventService ??
                                        throw new ArgumentNullException(nameof(imageIntegrationEventService));
        _settings = settings.Value;
        _identityService = identityService;
        
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
}