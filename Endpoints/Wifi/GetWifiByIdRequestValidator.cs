using FastEndpoints;
using FluentValidation;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class GetWifiByIdRequestValidator : Validator<GetWifiByIdRequest>
{
    public GetWifiByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("WiFi network ID is required");
    }
}
