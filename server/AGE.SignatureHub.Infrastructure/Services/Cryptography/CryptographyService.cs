using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;

namespace AGE.SignatureHub.Infrastructure.Services.Cryptography
{
    public class CryptographyService : ICryptographyService
    {
        private readonly byte[] _encryptionKey;
        public CryptographyService()
        {
            _encryptionKey = Encoding.UTF8.GetBytes("AGE_SignatureHub_Secret_Key_32bytes!"); 
        }

        public async Task<string> ComputeHashAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                throw new InvalidOperationException($"Error computing hash: {ex.Message}", ex);
            }
        }

        public Task<byte[]> DecryptAsync(byte[] encryptedData, CancellationToken cancellationToken = default)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _encryptionKey;

                using var msDecrypt = new MemoryStream(encryptedData);
                var iv = new byte[aes.BlockSize / 8];
                msDecrypt.Read(iv, 0, iv.Length); // Read the IV from the beginning of the stream
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var msPlain = new MemoryStream();
                csDecrypt.CopyTo(msPlain);

                return Task.FromResult(msPlain.ToArray());
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                throw new InvalidOperationException($"Error decrypting data: {ex.Message}", ex);
            }
        }

        public Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _encryptionKey;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var msEncrypt = new MemoryStream();
                msEncrypt.Write(aes.IV, 0, aes.IV.Length); // Prepend IV to the encrypted data

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(data, 0, data.Length);
                    csEncrypt.FlushFinalBlock();
                }

                return Task.FromResult(msEncrypt.ToArray());
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                throw new InvalidOperationException($"Error encrypting data: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateHashAsync(Stream stream, string expectedHash, CancellationToken cancellationToken = default)
        {
            try
            {
                var actualHash = await ComputeHashAsync(stream, cancellationToken);
                return actualHash.Equals(expectedHash, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                throw new InvalidOperationException($"Error validating hash: {ex.Message}", ex);
            }
        }
    }
}