namespace Review.API.IntegrationEvents;

public class ReviewIntegrationEventService : IReviewIntegrationEventService, IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly IIntegrationEventLogService _eventLogService;
    private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
    private readonly ILogger<ReviewIntegrationEventService> _logger;
    private readonly ReviewContext _reviewContext;
    private volatile bool disposedValue;

    public ReviewIntegrationEventService(
        ILogger<ReviewIntegrationEventService> logger,
        IEventBus eventBus,
        ReviewContext ReviewContext,
        Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reviewContext = ReviewContext ?? throw new ArgumentNullException(nameof(ReviewContext));
        _integrationEventLogServiceFactory = integrationEventLogServiceFactory ??
                                             throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _eventLogService = _integrationEventLogServiceFactory(_reviewContext.Database.GetDbConnection());
    }

    public void Dispose()
    {
        Dispose(true);
        // https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1816
        GC.SuppressFinalize(this);
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
            "----- ReviewIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);

        //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
        //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency            
        var strategy = _reviewContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _reviewContext.Database.BeginTransactionAsync();
            await _eventLogService.SaveEventAsync(evt, _reviewContext.Database.CurrentTransaction);
            await transaction.CommitAsync();
        });
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