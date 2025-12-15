namespace FastAPIHomeWifiQR.Endpoints.Wifi
{
    public class DownloadBulkQrCodesRequest
    {
        public Guid[] Ids { get; set; } = Array.Empty<Guid>();
    }
}
