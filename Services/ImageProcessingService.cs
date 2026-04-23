using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using Microsoft.Extensions.Logging;

namespace inmobiliariaApi.Services
{
    public class ImageProcessingService
    {
        private readonly ILogger<ImageProcessingService> _logger;

        public ImageProcessingService(ILogger<ImageProcessingService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Compresses and resizes the full image for storage.
        /// Max dimensions: 1920x1920 (maintains aspect ratio)
        /// Format: WebP at 82% quality
        /// </summary>
        public async Task<Stream> ComprimirAsync(Stream input, string originalContentType)
        {
            try
            {
                input.Position = 0;
                using var image = await Image.LoadAsync(input);

                // Auto-orient based on EXIF
                image.Mutate(x => x.AutoOrient());

                // Resize if needed (max 1920x1920, maintaining aspect ratio)
                int maxDimension = 1920;
                if (image.Width > maxDimension || image.Height > maxDimension)
                {
                    var ratio = Math.Min((double)maxDimension / image.Width, (double)maxDimension / image.Height);
                    int newWidth = (int)(image.Width * ratio);
                    int newHeight = (int)(image.Height * ratio);
                    image.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
                }

                // Save as WebP with 82% quality
                var output = new MemoryStream();
                var encoder = new WebpEncoder { Quality = 82 };
                await image.SaveAsWebpAsync(output, encoder);

                output.Position = 0;
                _logger.LogInformation("Image compressed successfully. Original: {OriginalContentType}, Output size: {Size} bytes",
                    originalContentType, output.Length);

                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing image");
                throw new InvalidOperationException("No se pudo procesar la imagen", ex);
            }
        }

        /// <summary>
        /// Generates a thumbnail for the image.
        /// Max width: 480px (height scales proportionally)
        /// Format: WebP at 72% quality
        /// </summary>
        public async Task<Stream> GenerarThumbnailAsync(Stream input)
        {
            try
            {
                input.Position = 0;
                using var image = await Image.LoadAsync(input);

                // Auto-orient based on EXIF
                image.Mutate(x => x.AutoOrient());

                // Resize to max 480px width, maintaining aspect ratio
                int maxWidth = 480;
                if (image.Width > maxWidth)
                {
                    double ratio = (double)maxWidth / image.Width;
                    int newHeight = (int)(image.Height * ratio);
                    image.Mutate(x => x.Resize(maxWidth, newHeight, KnownResamplers.Lanczos3));
                }

                // Save as WebP with 72% quality
                var output = new MemoryStream();
                var encoder = new WebpEncoder { Quality = 72 };
                await image.SaveAsWebpAsync(output, encoder);

                output.Position = 0;
                _logger.LogInformation("Thumbnail generated successfully. Size: {Size} bytes", output.Length);

                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail");
                throw new InvalidOperationException("No se pudo generar la miniatura", ex);
            }
        }
    }
}
