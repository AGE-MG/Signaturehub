using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.Contracts.Infrastructure
{
    public interface ICryptographyService
    {
        Task<string> ComputeHashAsync(Stream stream, CancellationToken cancellationToken = default);
        Task<bool> ValidateHashAsync(Stream stream, string expectedHash, CancellationToken cancellationToken = default);
        Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken = default);
        Task<byte[]> DecryptAsync(byte[] encryptedData, CancellationToken cancellationToken = default);
    }
}