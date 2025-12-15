using QRCoder;
using System.IO;
using System.Text;

namespace HomeWifiQR.Services
{
    public class QrCodeService : IQrCodeService
    {
        public string BuildWifiPayload(string ssid, string? password, string encryption, bool hidden)
        {
            // WIFI:T:WPA;S:MySSID;P:MyPass;H:true;;
            var sb = new StringBuilder();
            sb.Append("WIFI:");
            sb.Append($"T:{encryption};");
            sb.Append($"S:{Escape(ssid)};");
            if (!string.IsNullOrEmpty(password))
                sb.Append($"P:{Escape(password)};");
            if (hidden)
                sb.Append("H:true;");
            sb.Append(";");
            return sb.ToString();
        }

        public byte[] GenerateWifiPng(string ssid, string? password, string encryption, bool hidden)
        {
            var payload = BuildWifiPayload(ssid, password, encryption, hidden);

            using var gen = new QRCodeGenerator();
            using var qrData = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

            // Use QRCoder's PngByteQRCode to produce PNG bytes directly (cross-platform, avoids System.Drawing)
            var pngQr = new PngByteQRCode(qrData);
            return pngQr.GetGraphic(20);
        }

        private static string Escape(string input) =>
            input.Replace("\\", "\\\\").Replace(";", "\\;");
    }
}