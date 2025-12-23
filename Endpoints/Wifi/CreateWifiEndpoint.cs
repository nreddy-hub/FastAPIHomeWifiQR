using FastEndpoints;
using HomeWifiQR.Models;
using HomeWifiQR.Services;
using FastAPIHomeWifiQR.Services;
using FastAPIHomeWifiQR.Models;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class CreateWifiEndpoint : Endpoint<CreateWifiRequest, CreateWifiResponse>
{
    private readonly IWifiService _wifiService;
    private readonly IObjectMapper _mapper;
    private readonly ISqsService? _sqsService;
    private readonly ILogger<CreateWifiEndpoint> _logger;
    private readonly bool _sqsEnabled;

    public CreateWifiEndpoint(
        IWifiService wifiService,
        IObjectMapper mapper,
        ILogger<CreateWifiEndpoint> logger,
        IConfiguration configuration,
        ISqsService? sqsService = null)
    {
        _wifiService = wifiService;
        _mapper = mapper;
        _sqsService = sqsService;
        _logger = logger;
        _sqsEnabled = configuration.GetValue<bool>("AWS:SQS:EnableSqs");
        
        // Log configuration status
        _logger.LogInformation(
            "CreateWifiEndpoint initialized. SQS Enabled: {SqsEnabled}, SqsService: {SqsServiceStatus}",
            _sqsEnabled,
            _sqsService != null ? "Available" : "NULL");
    }

    public override void Configure()
    {
        Post("/api/wifi");
        // Require authentication - user must have a valid JWT token
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(CreateWifiRequest req, CancellationToken ct)
    {
        // Use AgileMapper to map request DTO to domain model
        var wifiNetwork = _mapper.Map<WifiNetwork>(req);

        var created = await _wifiService.CreateAsync(wifiNetwork, ct);

        // Send message to SQS if enabled
        if (_sqsEnabled && _sqsService != null)
        {
            try
            {
                // Create SQS message with custom mapping for WifiId property
                var message = new WifiQrCreatedMessage
                {
                    WifiId = created.Id, // Manual mapping: Id → WifiId
                    Ssid = created.Ssid,
                    Encryption = created.Encryption,
                    Hidden = created.Hidden,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User?.Identity?.Name ?? "System"
                };

                var messageId = await _sqsService.SendMessageAsync(message, cancellationToken: ct);
                
                _logger.LogInformation(
                    "WiFi QR code created and message sent to SQS. WifiId: {WifiId}, MessageId: {MessageId}",
                    created.Id,
                    messageId);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                _logger.LogError(ex, "Failed to send message to SQS for WifiId: {WifiId}", created.Id);
            }
        }
        else
        {
            _logger.LogInformation(
                "SQS is disabled or service unavailable. WiFi QR created without sending message. WifiId: {WifiId}",
                created.Id);
        }

        // Use AgileMapper to map domain model to response DTO
        var response = _mapper.Map<CreateWifiResponse>(created);
        await SendAsync(response, cancellation: ct);
    }
}
