namespace EventLog.Model;

public record EventLogItem
{
    private EventLogItem()
    {
    }

    public EventLogItem(IntegrationEvent @event, Guid transactionId)
    {
        EventId = @event.Id;
        CreationTime = @event.CreationDate;
        EventTypeName = @event.GetType().FullName;
        Content = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions
        {
            WriteIndented = true
        });
        State = EventStateEnum.NotPublished;
        TimesSent = 0;
        TransactionId = transactionId.ToString();
    }

    public Guid EventId { get; }
    public string EventTypeName { get; }
    public string? EventTypeShortName => EventTypeName.Split('.')?.Last();
    public IntegrationEvent? IntegrationEvent { get; private set; }
    public EventStateEnum State { get; set; }
    public int TimesSent { get; set; }
    public DateTime CreationTime { get; }
    public string Content { get; }
    public string TransactionId { get; }

    public EventLogItem DeserializeJsonContent(Type type)
    {
        IntegrationEvent =
            JsonSerializer.Deserialize(Content, type, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                as IntegrationEvent;
        return this;
    }
}