using HomeWifiQR.Data;
using HomeWifiQR.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.IO.Compression;

namespace HomeWifiQR.Services;

public class WifiService : IWifiService
{
    private readonly ApplicationDbContext _db;
    private readonly IQrCodeService _qr;

    public WifiService(ApplicationDbContext db, IQrCodeService qr)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _qr = qr ?? throw new ArgumentNullException(nameof(qr));
    }

    public async Task<List<WifiNetwork>> GetAllAsync(CancellationToken ct = default) =>
        await _db.WifiNetworks
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<WifiNetwork?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.WifiNetworks.FindAsync(new object[] { id }, cancellationToken: ct);

    public async Task<WifiNetwork> CreateAsync(WifiNetwork model, CancellationToken ct = default)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        model.Id = Guid.NewGuid();
        _db.WifiNetworks.Add(model);
        await _db.SaveChangesAsync(ct);
        return model;
    }

    public async Task<byte[]?> GenerateQrPngAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.WifiNetworks.FindAsync(new object[] { id }, cancellationToken: ct);
        if (entity == null) return null;

        // Build the standard WIFI payload and delegate PNG generation to IQrCodeService
        // e.g. WIFI:S:<ssid>;T:<WPA|WEP|nopass>;P:<password>;H:<true|false>;;
        // Delegate payload/PNG responsibilities to the QR service to avoid duplication
        return _qr.GenerateWifiPng(entity.Ssid, entity.Password, entity.Encryption, entity.Hidden);
    }

    public async Task<byte[]?> CreateZipAsync(Guid[] ids, CancellationToken ct = default)
    {
        if (ids == null || ids.Length == 0) return null;

        var items = await _db.WifiNetworks
            .Where(w => ids.Contains(w.Id))
            .AsNoTracking()
            .ToListAsync(ct);

        if (!items.Any()) return null;

        await using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var it in items)
            {
                ct.ThrowIfCancellationRequested();

                // Delegate payload/PNG responsibilities to the QR service to avoid duplication
                var png = _qr.GenerateWifiPng(it.Ssid, it.Password, it.Encryption, it.Hidden);
                var entryName = $"{Sanitize(it.Ssid)}_{it.Id}.png";
                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                await using var es = entry.Open();
                await es.WriteAsync(png, 0, png.Length, ct);
            }
        }

        ms.Position = 0;
        return ms.ToArray();
    }

    public async Task<List<WifiCredentialsDto>> GetAllCredentialsAsync(CancellationToken ct = default)
    {
        return await _db.WifiNetworks
            .AsNoTracking()
            .Select(w => new WifiCredentialsDto(w.Id, w.Ssid, w.Password))
            .ToListAsync(ct);
    }

    public async Task<WifiCredentialsDto?> GetCredentialsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.WifiNetworks
            .AsNoTracking()
            .Where(w => w.Id == id)
            .Select(w => new WifiCredentialsDto(w.Id, w.Ssid, w.Password))
            .FirstOrDefaultAsync(ct);
    }

    // Helper: sanitize file names for zip entries
    private static string Sanitize(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s;
    }
}