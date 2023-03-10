namespace Image.API.IntegrationEvents;

public class ImageIntegrationEventService : IImageIntegrationEventService, IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly IIntegrationEventLogService _eventLogService;
    private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
    private readonly ILogger<ImageIntegrationEventService> _logger;
    private readonly ImageContext _imageContext;
    private volatile bool disposedValue;

    public ImageIntegrationEventService(
        ILogger<ImageIntegrationEventService> logger,
        IEventBus eventBus,
        ImageContext ImageContext,
        Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _imageContext = ImageContext ?? throw new ArgumentNullException(nameof(ImageContext));
        _integrationEventLogServiceFactory = integrationEventLogServiceFactory ??
                                             throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _eventLogService = _integrationEventLogServiceFactory(_imageContext.Database.GetDbConnection());
    }

    public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
    {
        try
        {
            _logger.LogInformation(
                "----- Publishing integration event: {IntegrationEventId_published} from {AppName} - ({@IntegrationEvent})",
                evt.Id, Program.AppName, evt);

            await _eventLogService.MarkEventAsInProgressAsync(evt.Id);
            _eventBus.Publish(evt);
            await _eventLogService.MarkEventAsPublishedAsync(evt.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ERROR Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})",
                evt.Id, Program.AppName, evt);
            await _eventLogService.MarkEventAsFailedAsync(evt.Id);
        }
    }

    public async Task SaveEventAsync(IntegrationEvent evt)
    {
        _logger.LogInformation(
            "----- ImageIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);

        //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
        //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency            
        var strategy = _imageContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _imageContext.Database.BeginTransactionAsync();
            await _eventLogService.SaveEventAsync(evt, _imageContext.Database.CurrentTransaction);
            await transaction.CommitAsync();
        });
    }

    public void Dispose()
    {
        Dispose(true);
        // https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1816
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) (_eventLogService as IDisposable)?.Dispose();

            disposedValue = true;
        }
    }
}