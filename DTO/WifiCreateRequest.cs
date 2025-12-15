namespace HomeWifiQR.DTO
{
    public class WifiCreateRequest
    {
        public string Ssid { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string Encryption { get; set; } = "WPA";
        public bool Hidden { get; set; } = false;
    }
}