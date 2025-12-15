using FastEndpoints;
using FluentValidation;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class DownloadQrCodeRequestValidator : Validator<DownloadQrCodeRequest>
{
    public DownloadQrCodeRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("WiFi network ID is required");
    }
}
