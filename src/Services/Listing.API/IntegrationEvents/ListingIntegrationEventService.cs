using System.Transactions;

namespace Listing.API.Events;

public class ListingIntegrationEventService : IListingIntegrationEventService, IDisposable
{
    private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
    private readonly IEventBus _eventBus;
    private readonly ListingContext _listingContext;
    private readonly IIntegrationEventLogService _eventLogService;
    private readonly ILogger<ListingIntegrationEventService> _logger;
    private volatile bool disposedValue;

    public ListingIntegrationEventService(
        ILogger<ListingIntegrationEventService> logger,
        IEventBus eventBus,
        ListingContext listingContext,
        Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _listingContext = listingContext ?? throw new ArgumentNullException(nameof(listingContext));
        _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _eventLogService = _integrationEventLogServiceFactory(_listingContext.Database.GetDbConnection());
    }

    public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
    {
        try
        {
            _logger.LogInformation("----- Publishing integration event: {IntegrationEventId_published} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);

            await _eventLogService.MarkEventAsInProgressAsync(evt.Id);
            _eventBus.Publish(evt);
            await _eventLogService.MarkEventAsPublishedAsync(evt.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);
            await _eventLogService.MarkEventAsFailedAsync(evt.Id);
        }
    }

    public async Task SaveEventAsync(IntegrationEvent evt)
    {
        _logger.LogInformation("----- ListingIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);

        //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
        //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency            
        var strategy = _listingContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _listingContext.Database.BeginTransactionAsync();
            await _eventLogService.SaveEventAsync(evt, _listingContext.Database.CurrentTransaction);
            await transaction.CommitAsync();
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                (_eventLogService as IDisposable)?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        // https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1816
        GC.SuppressFinalize(this);
    }
}
