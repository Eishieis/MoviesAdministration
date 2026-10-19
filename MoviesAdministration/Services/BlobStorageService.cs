using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MoviesAdministration.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            string containerName =
                configuration["AzureStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "Azure Storage container name is missing.");

            var options = new BlobClientOptions(
                BlobClientOptions.ServiceVersion.V2026_06_06);

            var blobServiceClient = new BlobServiceClient(
                connectionString,
                options);

            _containerClient =
                blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadAsync(
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            await _containerClient.CreateIfNotExistsAsync(
                cancellationToken: cancellationToken);

            string extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            string blobName = $"{Guid.NewGuid():N}{extension}";

            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            await using Stream stream = file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                },
                cancellationToken);

            return blobName;
        }

        public async Task<BlobDownloadStreamingResult?> DownloadAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync(
                cancellationToken);

            if (!exists.Value)
            {
                return null;
            }

            var response = await blobClient.DownloadStreamingAsync(
                cancellationToken: cancellationToken);

            return response.Value;
        }
    }
}