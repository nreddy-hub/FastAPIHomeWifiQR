namespace FastAPIHomeWifiQR.Models;

/// <summary>
/// Message sent to SQS when a WiFi QR code is created
/// </summary>
public class WifiQrCreatedMessage
{
    public Guid WifiId { get; set; }
    public string Ssid { get; set; } = string.Empty;
    public string Encryption { get; set; } = string.Empty;
    public bool Hidden { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "System";
    
    /// <summary>
    /// Optional metadata for additional processing
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
