using System.ComponentModel.DataAnnotations;

namespace HomeWifiQR.Models;

public class WifiNetwork
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Ssid { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Required]
    public string Encryption { get; set; } = "WPA";

    public bool Hidden { get; set; }
}