using FastEndpoints;
using HomeWifiQR.Services;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class DownloadQrCodeEndpoint : Endpoint<DownloadQrCodeRequest>
{
    private readonly IWifiService _wifiService;

    public DownloadQrCodeEndpoint(IWifiService wifiService)
    {
        _wifiService = wifiService;
    }

    public override void Configure()
    {
        Get("/api/wifi/{id}/qr");
        // Require authentication - user must have a valid JWT token
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(DownloadQrCodeRequest req, CancellationToken ct)
    {
        var qrCodeBytes = await _wifiService.GenerateQrPngAsync(req.Id, ct);

        if (qrCodeBytes == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendBytesAsync(qrCodeBytes, "qrcode.png", contentType: "image/png", cancellation: ct);
    }
}
