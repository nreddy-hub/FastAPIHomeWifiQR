using FastEndpoints;
using HomeWifiQR.Services;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class GetAllWifiEndpoint : EndpointWithoutRequest<List<WifiNetworkResponse>>
{
    private readonly IWifiService _wifiService;

    public GetAllWifiEndpoint(IWifiService wifiService)
    {
        _wifiService = wifiService;
    }

    public override void Configure()
    {
        Get("/api/wifi");
        // Require authentication - user must have a valid JWT token
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var networks = await _wifiService.GetAllAsync(ct);

        var response = networks.Select(n => new WifiNetworkResponse
        {
            Id = n.Id,
            Ssid = n.Ssid,
            Password = n.Password,
            Encryption = n.Encryption,
            Hidden = n.Hidden
        }).ToList();

        await SendAsync(response, cancellation: ct);
    }
}
