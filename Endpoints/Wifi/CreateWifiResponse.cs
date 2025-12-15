namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class CreateWifiResponse
{
    public Guid Id { get; set; }
    public string Ssid { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Encryption { get; set; } = "WPA";
    public bool Hidden { get; set; }
}
