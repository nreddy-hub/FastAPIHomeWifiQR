using FastEndpoints;
using FastEndpoints.Swagger;
using FastEndpoints.Security;
using HomeWifiQR.Services;
using HomeWifiQR.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register your services
builder.Services.AddScoped<IWifiService, WifiService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IObjectMapper, AgileMapperAdapter>();

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

app.Run();
