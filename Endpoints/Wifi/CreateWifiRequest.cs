namespace FastAPIHomeWifiQR.Endpoints.Wifi;

public class CreateWifiRequest
{
    public string Ssid { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Encryption { get; set; } = "WPA";
    public bool Hidden { get; set; }
}
