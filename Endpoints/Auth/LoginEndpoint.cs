using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.Extensions.Configuration;

namespace FastAPIHomeWifiQR.Endpoints.Auth;

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly IConfiguration _configuration;

    public LoginEndpoint(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        // Simple hardcoded credentials validation (replace with your actual user validation)
        // You should validate against database or identity provider
        if (req.Username == "admin" && req.Password == "password123")
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var signingKey = jwtSettings["SigningKey"]!;
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]!);

            // Generate JWT token
            var jwtToken = JwtBearer.CreateToken(options =>
            {
                options.SigningKey = signingKey;
                options.ExpireAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
                options.Issuer = jwtSettings["Issuer"];
                options.Audience = jwtSettings["Audience"];
                // Add custom claims using the User property
                options.User["username"] = req.Username;
                options.User["role"] = "admin";
            });

            await SendAsync(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = jwtToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
            }, cancellation: ct);
        }
        else
        {
            await SendAsync(new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password"
            }, 401, ct);
        }
    }
}
