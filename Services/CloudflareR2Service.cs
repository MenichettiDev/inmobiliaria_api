using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;

namespace inmobiliariaApi.Services
{
    public class CloudflareR2Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _publicUrl;
        private readonly ILogger<CloudflareR2Service> _logger;

        public CloudflareR2Service(IConfiguration configuration, ILogger<CloudflareR2Service> logger)
        {
            _logger = logger;

            var accountId = configuration["Cloudflare:R2:AccountId"];
            var accessKeyId = configuration["Cloudflare:R2:AccessKeyId"];
            var secretAccessKey = configuration["Cloudflare:R2:SecretAccessKey"];
            _bucketName = configuration["Cloudflare:R2:BucketName"] ?? "inmobiliaria";
            _publicUrl = configuration["Cloudflare:R2:PublicUrl"] ?? "https://images.inmobiliaria.com";

            if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(secretAccessKey))
            {
                _logger.LogWarning("Cloudflare R2 credentials not fully configured.");
            }

            var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(credentials, config);
        }

        /// <summary>
        /// Generates a unique key for storing an image in R2
        /// Format: tenants/{tenantId}/propiedad/{propiedadId}/{guid}-{sanitized-filename}
        /// </summary>
        public string GenerateKey(int tenantId, int propiedadId, string fileName)
        {
            // Sanitize filename: remove special characters, keep only alphanumeric, dash, underscore, dot
            var sanitized = System.Text.RegularExpressions.Regex.Replace(
                Path.GetFileNameWithoutExtension(fileName),
                @"[^\w\-.]",
                "_"
            );
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueName = $"{Guid.NewGuid()}{extension}";

            return $"tenants/{tenantId}/propiedad/{propiedadId}/{uniqueName}";
        }

        /// <summary>
        /// Uploads a file to Cloudflare R2 and returns the public URL
        /// </summary>
        public async Task<string> UploadAsync(Stream stream, string key, string contentType)
        {
            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType,
                    DisablePayloadSigning = true  // R2 no soporta STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER
                };

                var response = await _s3Client.PutObjectAsync(putObjectRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK || response.HttpStatusCode == System.Net.HttpStatusCode.Created)
                {
                    var publicUrl = $"{_publicUrl.TrimEnd('/')}/{key}";
                    _logger.LogInformation("File uploaded to R2: {Key} -> {Url}", key, publicUrl);
                    return publicUrl;
                }

                throw new InvalidOperationException($"Upload failed with status: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to R2: {Key}", key);
                throw;
            }
        }

        /// <summary>
        /// Deletes a file from Cloudflare R2
        /// </summary>
        public async Task DeleteAsync(string key)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                _logger.LogInformation("File deleted from R2: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from R2: {Key}", key);
                throw;
            }
        }

        /// <summary>
        /// Checks if a file exists in Cloudflare R2
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if file exists in R2: {Key}", key);
                throw;
            }
        }
    }
}
