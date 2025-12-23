using FastEndpoints;
using HomeWifiQR.Services;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class GetWifiByIdEndpoint : Endpoint<GetWifiByIdRequest, WifiNetworkResponse>
{
    private readonly IWifiService _wifiService;
    private readonly IObjectMapper _mapper;

    public GetWifiByIdEndpoint(IWifiService wifiService, IObjectMapper mapper)
    {
        _wifiService = wifiService;
        _mapper = mapper;
    }

    public override void Configure()
    {
        Get("/api/wifi/{id}");
        // Require authentication - user must have a valid JWT token
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(GetWifiByIdRequest req, CancellationToken ct)
    {
        var network = await _wifiService.GetByIdAsync(req.Id, ct);

        if (network == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // Use AgileMapper to map domain model to response DTO
        var response = _mapper.Map<WifiNetworkResponse>(network);
        await SendAsync(response, cancellation: ct);
    }
}
