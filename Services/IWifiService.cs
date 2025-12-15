using HomeWifiQR.Models;

namespace HomeWifiQR.Services;

public interface IWifiService
{
    Task<List<WifiNetwork>> GetAllAsync(CancellationToken ct = default);
    Task<WifiNetwork?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WifiNetwork> CreateAsync(WifiNetwork model, CancellationToken ct = default);
    Task<byte[]?> GenerateQrPngAsync(Guid id, CancellationToken ct = default);
    Task<byte[]?> CreateZipAsync(Guid[] ids, CancellationToken ct = default);
    Task<List<WifiCredentialsDto>> GetAllCredentialsAsync(CancellationToken ct = default);
    Task<WifiCredentialsDto?> GetCredentialsByIdAsync(Guid id, CancellationToken ct = default);
}