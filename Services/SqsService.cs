using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;

namespace FastAPIHomeWifiQR.Services;

/// <summary>
/// Service for sending messages to Amazon SQS
/// </summary>
public class SqsService : ISqsService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly ILogger<SqsService> _logger;
    private readonly string _queueUrl;

    public SqsService(
        IAmazonSQS sqsClient,
        IConfiguration configuration,
        ILogger<SqsService> logger)
    {
        _sqsClient = sqsClient;
        _logger = logger;
        _queueUrl = configuration["AWS:SQS:QueueUrl"] 
            ?? throw new InvalidOperationException("SQS Queue URL is not configured");
    }

    /// <summary>
    /// Sends a message to the SQS queue
    /// </summary>
    public async Task<string> SendMessageAsync<T>(
        T message, 
        string? messageGroupId = null, 
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = messageBody,
                MessageGroupId = messageGroupId // Required for FIFO queues
            };

            // Add message deduplication ID for FIFO queues
            if (!string.IsNullOrEmpty(messageGroupId))
            {
                request.MessageDeduplicationId = Guid.NewGuid().ToString();
            }

            var response = await _sqsClient.SendMessageAsync(request, cancellationToken);

            _logger.LogInformation(
                "Successfully sent message to SQS. MessageId: {MessageId}, Type: {MessageType}",
                response.MessageId,
                typeof(T).Name);

            return response.MessageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to SQS. Message type: {MessageType}", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Sends multiple messages to the SQS queue in a batch (max 10 messages)
    /// </summary>
    public async Task<List<string>> SendMessageBatchAsync<T>(
        List<T> messages, 
        CancellationToken cancellationToken = default) where T : class
    {
        if (messages == null || messages.Count == 0)
        {
            return new List<string>();
        }

        // SQS batch limit is 10 messages
        const int batchSize = 10;
        var messageIds = new List<string>();

        try
        {
            for (int i = 0; i < messages.Count; i += batchSize)
            {
                var batch = messages.Skip(i).Take(batchSize).ToList();
                var entries = batch.Select((msg, index) => new SendMessageBatchRequestEntry
                {
                    Id = index.ToString(),
                    MessageBody = JsonSerializer.Serialize(msg, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                }).ToList();

                var request = new SendMessageBatchRequest
                {
                    QueueUrl = _queueUrl,
                    Entries = entries
                };

                var response = await _sqsClient.SendMessageBatchAsync(request, cancellationToken);

                messageIds.AddRange(response.Successful.Select(s => s.MessageId));

                if (response.Failed.Any())
                {
                    _logger.LogWarning(
                        "Some messages failed to send. Failed count: {FailedCount}",
                        response.Failed.Count);
                }

                _logger.LogInformation(
                    "Sent batch of {Count} messages to SQS. Successful: {SuccessCount}, Failed: {FailedCount}",
                    batch.Count,
                    response.Successful.Count,
                    response.Failed.Count);
            }

            return messageIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message batch to SQS");
            throw;
        }
    }
}
