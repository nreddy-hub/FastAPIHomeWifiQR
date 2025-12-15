using FastEndpoints;
using HomeWifiQR.Models;
using HomeWifiQR.Services;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class CreateWifiEndpoint : Endpoint<CreateWifiRequest, CreateWifiResponse>
{
    private readonly IWifiService _wifiService;

    public CreateWifiEndpoint(IWifiService wifiService)
    {
        _wifiService = wifiService;
    }

    public override void Configure()
    {
        Post("/api/wifi");
        // Require authentication - user must have a valid JWT token
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(CreateWifiRequest req, CancellationToken ct)
    {
        var wifiNetwork = new WifiNetwork
        {
            Ssid = req.Ssid,
            Password = req.Password,
            Encryption = req.Encryption,
            Hidden = req.Hidden
        };

        var created = await _wifiService.CreateAsync(wifiNetwork, ct);

        await SendAsync(new CreateWifiResponse
        {
            Id = created.Id,
            Ssid = created.Ssid,
            Password = created.Password,
            Encryption = created.Encryption,
            Hidden = created.Hidden
        }, cancellation: ct);
    }
}
