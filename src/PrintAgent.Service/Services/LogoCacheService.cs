using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PrintAgent.Service.Services;

/// <summary>
/// Servicio para cachear y convertir logos a formato ESC/POS
/// </summary>
public class LogoCacheService : ILogoCacheService
{
    private readonly ILogger<LogoCacheService> _logger;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, CachedLogo> _cache = new();
    private readonly object _lock = new();

    // Tamaño máximo del logo para impresoras térmicas (en píxeles)
    private const int MaxLogoWidth = 384; // 48mm * 8 dots/mm para impresoras de 58mm
    private const int MaxLogoHeight = 200;

    // Tiempo de expiración del caché
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    public LogoCacheService(ILogger<LogoCacheService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    /// <summary>
    /// Obtiene el logo en formato ESC/POS bitmap, usando caché si está disponible
    /// </summary>
    public async Task<byte[]?> GetLogoBitmapAsync(string? logoUrl)
    {
        if (string.IsNullOrEmpty(logoUrl))
            return null;

        try
        {
            // Verificar caché
            lock (_lock)
            {
                if (_cache.TryGetValue(logoUrl, out var cached))
                {
                    if (DateTime.Now - cached.CachedAt < CacheExpiration)
                    {
                        _logger.LogDebug("Logo obtenido del caché: {Url}", logoUrl);
                        return cached.BitmapData;
                    }
                    // Caché expirado, remover
                    _cache.Remove(logoUrl);
                }
            }

            // Descargar imagen
            _logger.LogInformation("Descargando logo: {Url}", logoUrl);
            var imageBytes = await _httpClient.GetByteArrayAsync(logoUrl);

            // Convertir a bitmap ESC/POS
            var bitmapData = ConvertToEscPosBitmap(imageBytes);

            if (bitmapData != null)
            {
                // Guardar en caché
                lock (_lock)
                {
                    _cache[logoUrl] = new CachedLogo
                    {
                        BitmapData = bitmapData,
                        CachedAt = DateTime.Now
                    };
                }
                _logger.LogInformation("Logo cacheado exitosamente: {Url}", logoUrl);
            }

            return bitmapData;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener logo: {Url}", logoUrl);
            return null;
        }
    }

    /// <summary>
    /// Convierte una imagen a formato bitmap ESC/POS (monocromático)
    /// </summary>
    private byte[] ConvertToEscPosBitmap(byte[] imageBytes)
    {
        using var image = Image.Load<Rgba32>(imageBytes);

        // Redimensionar si es necesario
        if (image.Width > MaxLogoWidth || image.Height > MaxLogoHeight)
        {
            var ratio = Math.Min((double)MaxLogoWidth / image.Width, (double)MaxLogoHeight / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // El ancho debe ser múltiplo de 8 para ESC/POS
        var width = image.Width;
        var height = image.Height;
        var widthBytes = (width + 7) / 8;

        // Crear datos del bitmap
        var bitmapData = new List<byte>();

        // Comando ESC/POS para imprimir bitmap
        // GS v 0 - Print raster bit image
        bitmapData.Add(0x1D); // GS
        bitmapData.Add(0x76); // v
        bitmapData.Add(0x30); // 0
        bitmapData.Add(0x00); // m = 0 (normal mode)

        // xL, xH - número de bytes por línea
        bitmapData.Add((byte)(widthBytes & 0xFF));
        bitmapData.Add((byte)((widthBytes >> 8) & 0xFF));

        // yL, yH - número de líneas
        bitmapData.Add((byte)(height & 0xFF));
        bitmapData.Add((byte)((height >> 8) & 0xFF));

        // Convertir imagen a monocromático y generar datos
        for (int y = 0; y < height; y++)
        {
            for (int xByte = 0; xByte < widthBytes; xByte++)
            {
                byte b = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    int x = xByte * 8 + bit;
                    if (x < width)
                    {
                        var pixel = image[x, y];
                        // Convertir a escala de grises y aplicar umbral
                        // Usar fórmula de luminosidad: 0.299*R + 0.587*G + 0.114*B
                        var gray = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                        // Considerar transparencia (fondo blanco)
                        if (pixel.A < 128)
                            gray = 255;

                        // Umbral: si es oscuro, imprimir punto (1)
                        if (gray < 128)
                        {
                            b |= (byte)(0x80 >> bit);
                        }
                    }
                }
                bitmapData.Add(b);
            }
        }

        return bitmapData.ToArray();
    }

    /// <summary>
    /// Limpia el caché de logos
    /// </summary>
    public void ClearCache()
    {
        lock (_lock)
        {
            _cache.Clear();
        }
        _logger.LogInformation("Caché de logos limpiado");
    }

    /// <summary>
    /// Limpia entradas expiradas del caché
    /// </summary>
    public void CleanExpiredCache()
    {
        lock (_lock)
        {
            var expiredKeys = _cache
                .Where(kvp => DateTime.Now - kvp.Value.CachedAt >= CacheExpiration)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogInformation("Limpiadas {Count} entradas expiradas del caché", expiredKeys.Count);
            }
        }
    }

    private class CachedLogo
    {
        public byte[] BitmapData { get; set; } = Array.Empty<byte>();
        public DateTime CachedAt { get; set; }
    }
}

public interface ILogoCacheService
{
    Task<byte[]?> GetLogoBitmapAsync(string? logoUrl);
    void ClearCache();
    void CleanExpiredCache();
}
