using FastEndpoints;
using FastEndpoints.Swagger;
using FastEndpoints.Security;
using HomeWifiQR.Services;
using HomeWifiQR.Data;
using Microsoft.EntityFrameworkCore;
using Amazon.SQS;
using FastAPIHomeWifiQR.Services;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/wifi-qr-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting WiFi QR Generator API...");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Register your services
    builder.Services.AddScoped<IWifiService, WifiService>();
    builder.Services.AddScoped<IQrCodeService, QrCodeService>();
    builder.Services.AddScoped<IObjectMapper, AgileMapperAdapter>();

    // Register AWS SQS Service
    var enableSqs = builder.Configuration.GetValue<bool>("AWS:SQS:EnableSqs");
    if (enableSqs)
    {
        // Configure AWS SDK
        builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonSQS>();
        builder.Services.AddScoped<ISqsService, SqsService>();
        Log.Information("AWS SQS integration enabled");
    }

    // Add CORS for React app
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173",  // Vite default
                "http://localhost:3000"   // Create React App default
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });

    // Add JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    builder.Services
        .AddAuthenticationJwtBearer(s => s.SigningKey = jwtSettings["SigningKey"]!)
        .AddAuthorization();

    // Add FastEndpoints
    builder.Services.AddFastEndpoints();

    // Add Swagger support with JWT
    builder.Services.SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Home WiFi QR API";
            s.Version = "v1";
            s.AddAuth("Bearer", new()
            {
                Type = NSwag.OpenApiSecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token"
            });
        };
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseHttpsRedirection();

    // Use CORS (MUST be before Authentication/Authorization)
    app.UseCors("AllowReactApp");

    // Use Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Use FastEndpoints
    app.UseFastEndpoints();

    // Use Swagger
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerGen();
    }

    Log.Information("API started successfully!");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
