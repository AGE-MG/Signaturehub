using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Domain.Enums;
using AGE.SignatureHub.Domain.ValueObjects;
using iText.Kernel.Crypto;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using AlterCertificateInfo = AGE.SignatureHub.Domain.ValueObjects.CertificateInfo;
using Org.BouncyCastle.Crypto;
using iText.Commons.Bouncycastle.Cert;
using iText.Commons.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Bouncycastle.Crypto;
using iText.Forms.Form.Element;

namespace AGE.SignatureHub.Infrastructure.Services.Signature
{
    public class SignatureService : ISignatureService
    {
        private readonly ILogger<SignatureService> _logger;
        private readonly ICryptographyService _cryptographyService;
        public SignatureService(ILogger<SignatureService> logger, ICryptographyService cryptographyService)
        {
            _logger = logger;
            _cryptographyService = cryptographyService;
        }
        public Task<byte[]> ComputeHashAsync(Stream documentStream, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> SignDocumentAsync(Stream documentStream, SignatureType signatureType, AlterCertificateInfo certificateInfo, SignatureMetadata metadata, CancellationToken cancellationToken = default)
        {
            try
            {
                return signatureType switch
                {
                    SignatureType.Eletronic => SignEletronicallyAsync(documentStream, metadata, cancellationToken),
                    SignatureType.DigitalA1 or SignatureType.DigitalA3 => SignWithCertficateAsync(documentStream, certificateInfo, metadata, cancellationToken),
                    _ => throw new ArgumentException("Tipo de assinatura não suportado.", nameof(signatureType))
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error signing document with signature type {signatureType}");
                throw;
            }
        }

        public Task<AlterCertificateInfo> ValidateCertificateAsync(byte[] certificateData, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateSignatureAsync(Stream signedDocumentStream, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async Task<byte[]> SignEletronicallyAsync(Stream documentStream, SignatureMetadata metadata, CancellationToken cancellationToken = default)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var pdfReader = new PdfReader(documentStream);
                using var pdfWriter = new PdfWriter(memoryStream);
                using var pdfDocument = new PdfDocument(pdfReader, pdfWriter);

                var info = pdfDocument.GetDocumentInfo();
                info.AddCreationDate();
                info.SetCreator("AGE SignatureHub");

                var catalog = pdfDocument.GetCatalog();
                var customMetadata = new PdfDictionary();
                customMetadata.Put(new PdfName("SignedAt"), new PdfString(DateTime.UtcNow.ToString("o")));
                customMetadata.Put(new PdfName("IpAddress"), new PdfString(metadata.IpAddress ?? "Unknown"));
                customMetadata.Put(new PdfName("UserAgent"), new PdfString(metadata.UserAgent ?? "Unknown"));
                customMetadata.Put(new PdfName("DocumentHash"), new PdfString(metadata.DocumentHash ?? "Unknown"));
                customMetadata.Put(new PdfName("SignatureType"), new PdfString("Electronic"));
                catalog.GetPdfObject().Put(new PdfName("CustomMetadata"), customMetadata);
                pdfDocument.Close();

                _logger.LogInformation("Document signed electronically.");

                return await Task.FromResult(memoryStream.ToArray());
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error signing document electronically.");
                throw;
            }
        }

        private async Task<byte[]> SignWithCertficateAsync(Stream documentStream, AlterCertificateInfo certificateInfo, SignatureMetadata metadata, CancellationToken cancellationToken = default)
        {
            try
            {
                var certificate = X509CertificateLoader.LoadPkcs12(
                    certificateInfo.RawData,
                    certificateInfo.Password,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

                var bcCert = DotNetUtilities.FromX509Certificate(certificate);
                var bcPrivateKey = DotNetUtilities.GetKeyPair(certificate.PrivateKey).Private;
                
                var itextCert = new X509CertificateBC(bcCert);
                var itextCertChain = new IX509Certificate[] { itextCert };
                var itextPrivateKey = new PrivateKeyBC(bcPrivateKey);

                using var memoryStream = new MemoryStream();
                var pdfReader = new PdfReader(documentStream);
                var stamperProperties = new StampingProperties().UseAppendMode();

                var signerProperties = new SignerProperties();
                var fieldName = $"Signature_{DateTime.UtcNow:yyyyMMddHHmmss}";
                
                signerProperties
                    .SetFieldName(fieldName)
                    .SetReason("Assinatura Digital")
                    .SetLocation(metadata.Location ?? "Brasil")
                    .SetSignatureCreator("AGE SignatureHub");
            
                var appearance = new SignatureFieldAppearance(fieldName)
                    .SetContent("Assinado digitalmente por AGE SignatureHub");
                
                signerProperties
                    .SetPageNumber(1)
                    .SetPageRect(new Rectangle(36, 648, 200, 100))
                    .SetSignatureAppearance(appearance);

                var signer = new PdfSigner(pdfReader, memoryStream, stamperProperties);
                signer.SetSignerProperties(signerProperties);

                IExternalSignature externalSignature = new PrivateKeySignature(itextPrivateKey, DigestAlgorithms.SHA256);

                // Assinar o documento usando PAdES (PDF Advanced Electronic Signatures)
                signer.SignDetached(
                    externalSignature,
                    itextCertChain,
                    null, // CRL (Certificate Revocation List) - opcional
                    null, // OCSP (Online Certificate Status Protocol) - opcional
                    null, // TSA (Time Stamp Authority) - opcional
                    0,
                    PdfSigner.CryptoStandard.CADES); // CAdES = PAdES-BES compatível com ICP-Brasil

                _logger.LogInformation(
                    "Document signed with certificate. Subject: {Subject}, Valid until: {ValidTo}",
                    certificateInfo.SubjectName,
                    certificateInfo.ValidTo);

                return await Task.FromResult(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing document with certificate");
                throw new InvalidOperationException(
                    "Falha ao assinar documento com certificado digital. Verifique se o certificado é válido e contém chave privada.",
                    ex);
            }
        }
    }
}