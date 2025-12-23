using FastEndpoints;
using HomeWifiQR.Services;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class GetAllWifiEndpoint : EndpointWithoutRequest<List<WifiNetworkResponse>>
{
    private readonly IWifiService _wifiService;
    private readonly IObjectMapper _mapper;

    public GetAllWifiEndpoint(IWifiService wifiService, IObjectMapper mapper)
    {
        _wifiService = wifiService;
        _mapper = mapper;
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

        // Use AgileMapper to map collection
        var response = networks.Select(n => _mapper.Map<WifiNetworkResponse>(n)).ToList();

        await SendAsync(response, cancellation: ct);
    }
}
