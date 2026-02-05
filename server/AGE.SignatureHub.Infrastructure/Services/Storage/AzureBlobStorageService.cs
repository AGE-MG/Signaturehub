using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Infrastructure.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace AGE.SignatureHub.Infrastructure.Services.Storage
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly StorageSettings _storageSettings;
        public AzureBlobStorageService(IOptions<StorageSettings> settings)
        {
            _storageSettings = settings.Value;
            _blobServiceClient = new BlobServiceClient(_storageSettings.ConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(_storageSettings.ContainerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(filePath);
                return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Error deleting file from Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(filePath);
                if (!await blobClient.ExistsAsync(cancellationToken))
                {
                    throw new FileNotFoundException($"The file '{filePath}' does not exist in the blob storage.");
                }

                var Stream = new MemoryStream();
                await blobClient.DownloadToAsync(Stream, cancellationToken);
                Stream.Position = 0; // Reset stream position

                return Stream;
            }
            catch (System.Exception ex)
            {
                
                throw new Exception($"Error downloading file from Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(filePath);
                return await blobClient.ExistsAsync(cancellationToken);
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Error checking file existence in Azure Blob Storage: {ex.Message}");
            }
        }

        public  async Task<string> GetFileUrlAsync(string filePath, TimeSpan expiresIn, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(filePath);
                if (!await blobClient.ExistsAsync(cancellationToken))
                {
                    throw new FileNotFoundException($"The file '{filePath}' does not exist in the blob storage.");
                }

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _storageSettings.ContainerName,
                    BlobName = filePath,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Error generating file URL from Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobName = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";
                var blobClient = _containerClient.GetBlobClient(blobName);

                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                };

                await blobClient.UploadAsync(fileStream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                }, cancellationToken);
                return blobName;
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Error uploading file to Azure Blob Storage: {ex.Message}");
            }
        }
    }
}