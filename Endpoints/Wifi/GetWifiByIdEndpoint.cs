using FastEndpoints;
using HomeWifiQR.Services;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class GetWifiByIdEndpoint : Endpoint<GetWifiByIdRequest, WifiNetworkResponse>
{
    private readonly IWifiService _wifiService;

    public GetWifiByIdEndpoint(IWifiService wifiService)
    {
        _wifiService = wifiService;
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

        await SendAsync(new WifiNetworkResponse
        {
            Id = network.Id,
            Ssid = network.Ssid,
            Password = network.Password,
            Encryption = network.Encryption,
            Hidden = network.Hidden
        }, cancellation: ct);
    }
}
