using Image.API.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Minio.Exceptions;

namespace Image.API.Services;

public class ImageService : IImageService
{
    private static MinioClient minio;
    private static Policy strategy;
    private readonly ImageSettings _settings;
    private readonly ILogger<ImageService> _logger;

    private string[] permittedExtensions = { ".jpeg", ".jpg", ".png" };

    private static readonly Dictionary<string, List<byte[]>> _fileSignature =
        new()
        {
            {
                ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            {
                ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
        };


    public ImageService(ImageSettings settings, ILogger<ImageService> logger, int _retryCount = 5)
    {
        _settings = settings;
        _logger = logger;

        minio = new MinioClient()
            .WithEndpoint(_settings.OSSEndpoint)
            .WithCredentials(_settings.OSSAccKey, _settings.OSSSecKey)
            .WithSSL(_settings.OSSSecure)
            .Build();

        strategy = Policy.Handle<MinioException>()
            .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time, fileName) =>
                {
                    _logger.LogWarning(ex,
                        $"Could not upload image {fileName} after {($"{time.TotalSeconds:n1}")}s ({ex.Message})");
                });
    }

    public bool ImageValidation()
    {
        throw new NotImplementedException("Check If an Image is Valid, not implemented yet");
    }

    public async Task<bool> ListingImageUpload(MultipartReader reader, int listingId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ReviewImageUpload(MultipartReader reader, int reviewId)
    {
        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition);

            if (hasContentDispositionHeader)
            {
                // This check assumes that there's a file
                // present without form data. If form data
                // is present, this method immediately fails
                // and returns the model error.
                if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                {
                    return false;
                }
                else
                {
                    // Don't trust the file name sent by the client. To display
                    // the file name, HTML-encode the value.
                    var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                        contentDisposition.FileName.Value);
                    var trustedFileNameForFileStorage = Path.GetRandomFileName();

                    // **WARNING!**
                    // In the following example, the file is saved without
                    // scanning the file's contents. In most production
                    // scenarios, an anti-virus/anti-malware scanner API
                    // is used on the file before making the file available
                    // for download or for use by other systems. 
                    // For more information, see the topic that accompanies 
                    // this sample.
                    var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                        section, contentDisposition, 
                        permittedExtensions, _settings.fileSizeLimit);
                    await SaveToMino(reviewId, trustedFileNameForFileStorage, streamedFileContent, section.ContentType);
                }
            }

            // Drain any remaining section body that hasn't been consumed and
            // read the headers for the next section.
            section = await reader.ReadNextSectionAsync();
        }
        return true;
    }

    private async Task SaveToMino(int reviewId, string fileName, byte[] imageData, string contentType)
    {
        var bucket = $"Review-{reviewId}";
        
        await strategy.Execute(async () =>
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(bucket);
            var found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(bucket);
                await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            }
        
            // Upload a file to bucket.
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucket)
                .WithRequestBody(imageData)
                .WithFileName(fileName)
                .WithContentType(contentType);
            await Task.Yield();
            await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
        });
        _logger.LogInformation($"Successfully uploaded {fileName}");
    }

    public bool ImageDelete()
    {
        throw new NotImplementedException();
    }

    public bool ImageUpdate()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ImageResize(string listingName, string fileName)
    {
        throw new NotImplementedException();
    }
}