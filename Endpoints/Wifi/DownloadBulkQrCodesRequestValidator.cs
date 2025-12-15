using FastEndpoints;
using FluentValidation;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class DownloadBulkQrCodesRequestValidator : Validator<DownloadBulkQrCodesRequest>
{
    public DownloadBulkQrCodesRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("At least one WiFi network ID is required");

        RuleFor(x => x.Ids)
            .Must(ids => ids.Length <= 50)
            .WithMessage("Cannot download more than 50 QR codes at once")
            .When(x => x.Ids != null && x.Ids.Length > 0);

        RuleForEach(x => x.Ids)
            .NotEmpty()
            .WithMessage("WiFi network ID cannot be empty");
    }
}
