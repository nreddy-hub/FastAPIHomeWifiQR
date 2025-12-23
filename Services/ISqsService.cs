namespace FastAPIHomeWifiQR.Services;

/// <summary>
/// Service interface for Amazon SQS operations
/// </summary>
public interface ISqsService
{
    /// <summary>
    /// Sends a message to the specified SQS queue
    /// </summary>
    Task<string> SendMessageAsync<T>(T message, string? messageGroupId = null, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Sends multiple messages to the SQS queue in a batch
    /// </summary>
    Task<List<string>> SendMessageBatchAsync<T>(List<T> messages, CancellationToken cancellationToken = default) where T : class;
}
