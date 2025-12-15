using FastEndpoints;
using FluentValidation;

namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class CreateWifiRequestValidator : Validator<CreateWifiRequest>
{
    public CreateWifiRequestValidator()
    {
        RuleFor(x => x.Ssid)
            .NotEmpty()
            .WithMessage("SSID is required")
            .MaximumLength(32)
            .WithMessage("SSID cannot exceed 32 characters");

        RuleFor(x => x.Password)
            .MaximumLength(63)
            .WithMessage("Password cannot exceed 63 characters")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .WithMessage("Password cannot be less than 8 characters")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Encryption)
            .NotEmpty()
            .WithMessage("Encryption type is required")
            .Must(x => new[] { "WPA", "WPA2", "WPA3", "WEP", "nopass" }.Contains(x))
            .WithMessage("Encryption must be one of: WPA, WPA2, WPA3, WEP, or nopass");

        // Password required for encrypted networks
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required for encrypted networks")
            .When(x => x.Encryption != "nopass");

        // Password should be empty for open networks
        RuleFor(x => x.Password)
            .Empty()
            .WithMessage("Password should not be set for open networks")
            .When(x => x.Encryption == "nopass");
    }
}
