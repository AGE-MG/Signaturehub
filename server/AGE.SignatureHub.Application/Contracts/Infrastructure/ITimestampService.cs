using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.Contracts.Infrastructure
{
    public interface ITimestampService
    {
        Task<byte[]> ApplyTimestampAsync(
            byte[] documentHash,
            CancellationToken cancellationToken = default);

        Task<bool> ValidateTimestampAsync(
            byte[] timestampToken,
            CancellationToken cancellationToken = default);
    }
}