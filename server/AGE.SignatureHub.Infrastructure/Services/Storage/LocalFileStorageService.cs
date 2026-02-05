using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AGE.SignatureHub.Infrastructure.Services.Storage
{
    public class LocalFileStorageService : IStorageService
    {
        private readonly StorageSettings _storageSettings;
        private readonly string _basePath;
        public LocalFileStorageService(IOptions<StorageSettings> settings)
        {
            _storageSettings = settings.Value;
            _basePath = _storageSettings.LocalPath ?? Path.Combine(Directory.GetCurrentDirectory(), "Storage");
            
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }
        public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Error deleting file from local storage: {ex.Message}");
            }
        }

        public async Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"The file '{filePath}' does not exist in local storage.");
                }
    
                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream, cancellationToken);
                }
                memoryStream.Position = 0; // Reset stream position
                return memoryStream;
            }
            catch (System.Exception ex)
            {
                
                throw new Exception($"Error downloading file from local storage: {ex.Message}");
            }
        }

        public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                return Task.FromResult(File.Exists(fullPath));
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Error checking file existence in local storage: {ex.Message}");
            }
        }

        public Task<string> GetFileUrlAsync(string filePath, TimeSpan expiresIn, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"The file '{filePath}' does not exist in local storage.");
                }
                return Task.FromResult(fullPath);
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Error getting file URL from local storage: {ex.Message}");
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                var dateFolder = DateTime.UtcNow.ToString("yyyy/MM/dd");
                var folderPath = Path.Combine(_basePath, dateFolder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(dateFolder, $"{Guid.NewGuid()}_{fileName}");
                var fullPath = Path.Combine(_basePath, filePath);
                using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
                }
                return filePath;
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Error uploading file to local storage: {ex.Message}");
            }
        }
    }
}