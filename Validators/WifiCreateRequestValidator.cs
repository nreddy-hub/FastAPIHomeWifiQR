using System;
using System.Linq;
using FluentValidation;
using HomeWifiQR.DTO;

namespace HomeWifiQR.Validators;

public class WifiCreateRequestValidator : AbstractValidator<WifiCreateRequest>
{
    public WifiCreateRequestValidator()
    {
        RuleFor(x => x.Ssid)
            .NotEmpty().WithMessage("SSID is required.")
            .MaximumLength(128).WithMessage("SSID must be 128 characters or fewer.");

        RuleFor(x => x.Encryption)
            .NotEmpty().WithMessage("Encryption is required.")
            .Must(e => new[] { "WPA", "WEP", "nopass" }.Contains(e, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Encryption must be one of: WPA, WEP, nopass.");

        When(x => !string.Equals(x.Encryption, "nopass", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required when encryption is not 'nopass'.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        });
    }
}