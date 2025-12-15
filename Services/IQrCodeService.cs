namespace HomeWifiQR.Services
{
    public interface IQrCodeService
    {
        string BuildWifiPayload(string ssid, string? password, string encryption, bool hidden);
        byte[] GenerateWifiPng(string ssid, string? password, string encryption, bool hidden);
    }
}