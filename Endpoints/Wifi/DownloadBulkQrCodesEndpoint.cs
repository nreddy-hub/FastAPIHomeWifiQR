using FastEndpoints;
using HomeWifiQR.Services;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class DownloadBulkQrCodesEndpoint : Endpoint<DownloadBulkQrCodesRequest>
{
    private readonly IWifiService _wifiService;

    public DownloadBulkQrCodesEndpoint(IWifiService wifiService)
    {
        _wifiService = wifiService;
    }

    public override void Configure()
    {
        Post("/api/wifi/bulk-qr");
        // Require authentication - user must have a valid JWT token
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(DownloadBulkQrCodesRequest req, CancellationToken ct)
    {
        if (req.Ids == null || req.Ids.Length == 0)
        {
            await SendAsync(new { error = "No IDs provided" }, 400, ct);
            return;
        }

        var zipBytes = await _wifiService.CreateZipAsync(req.Ids, ct);

        if (zipBytes == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendBytesAsync(zipBytes, "wifi-qrcodes.zip", contentType: "application/zip", cancellation: ct);
    }
}
