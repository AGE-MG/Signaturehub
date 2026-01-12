using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Enums;
using AGE.SignatureHub.Domain.ValueObjects;

namespace AGE.SignatureHub.Application.Contracts.Infrastructure
{
    public interface ISignatureService
    {
        Task<byte[]> SignDocumentAsync(
            Stream documentStream,
            SignatureType signatureType,
            CertificateInfo certificateInfo,
            SignatureMetadata metadata,
            CancellationToken cancellationToken = default);
        
        Task<bool> ValidateSignatureAsync(
            Stream signedDocumentStream,
            CancellationToken cancellationToken = default);
        
        Task<CertificateInfo> ValidateCertificateAsync(
            byte[] certificateData                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    ,
            CancellationToken cancellationToken = default);
    }
}