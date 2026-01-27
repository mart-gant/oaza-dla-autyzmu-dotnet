using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace OazaDlaAutyzmu.Infrastructure.Services;

public interface IImageService
{
    Task<string> SaveImageAsync(Stream imageStream, string fileName, string uploadPath);
    Task<bool> OptimizeImageAsync(string filePath, int maxWidth = 1920, int maxHeight = 1080, int quality = 85);
    Task DeleteImageAsync(string filePath);
    bool IsValidImageFormat(string fileName);
    long GetFileSizeInBytes(Stream stream);
}

public class ImageService : IImageService
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public bool IsValidImageFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    public long GetFileSizeInBytes(Stream stream)
    {
        return stream.Length;
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName, string uploadPath)
    {
        // Validate
        if (!IsValidImageFormat(fileName))
        {
            throw new ArgumentException("Invalid image format. Allowed: jpg, jpeg, png, gif, webp");
        }

        if (GetFileSizeInBytes(imageStream) > MaxFileSizeBytes)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSizeBytes / 1024 / 1024}MB");
        }

        // Create directory if it doesn't exist
        Directory.CreateDirectory(uploadPath);

        // Generate unique filename
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        // Save file
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            imageStream.Position = 0;
            await imageStream.CopyToAsync(fileStream);
        }

        // Optimize image
        await OptimizeImageAsync(filePath);

        return uniqueFileName;
    }

    public async Task<bool> OptimizeImageAsync(string filePath, int maxWidth = 1920, int maxHeight = 1080, int quality = 85)
    {
        try
        {
            using var image = await Image.LoadAsync(filePath);
            
            // Resize if larger than max dimensions
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = ResizeMode.Max
                }));
            }

            // Save with compression
            var encoder = new JpegEncoder
            {
                Quality = quality
            };

            await image.SaveAsync(filePath, encoder);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IMAGE OPTIMIZATION ERROR] {filePath}: {ex.Message}");
            return false;
        }
    }

    public Task DeleteImageAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IMAGE DELETE ERROR] {filePath}: {ex.Message}");
            throw;
        }
    }
}
