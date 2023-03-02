namespace EventLog.Infrastructure;

public class IntegretionEventLogContext : DbContext
{
    public IntegretionEventLogContext(DbContextOptions<IntegretionEventLogContext> options) : base(options) { }
    public DbSet<EventLogItem> EventLogs { get; set; }
}