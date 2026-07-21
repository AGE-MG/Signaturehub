using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Domain.Enums;
using AGE.SignatureHub.Domain.ValueObjects;
using iText.Barcodes;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.Kernel.Crypto;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Signatures;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
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
using LayoutCanvas = iText.Layout.Canvas;

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
        public async Task<byte[]> ComputeHashAsync(Stream documentStream, CancellationToken cancellationToken = default)
        {
            try
            {
                var hashString = await _cryptographyService.ComputeHashAsync(documentStream);
                var hash = ConvertHashStringToBytes(hashString);
                _logger.LogInformation("Document hash computed successfully.");
                return hash;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error computing document hash.");
                throw;
            }
        }

        private byte[] ConvertHashStringToBytes(string hashString)
        {
            if (string.IsNullOrWhiteSpace(hashString))
                throw new ArgumentException("Hash string cannot be null or empty.", nameof(hashString));

            
            hashString = hashString.Trim();

            if (System.Text.RegularExpressions.Regex.IsMatch(hashString, @"^[0-9a-fA-F]+$") && hashString.Length % 2 == 0)
            {
                try
                {
                    return Convert.FromHexString(hashString);
                }
                catch
                {
                    // Se falhar, continua para próximo formato
                }
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(hashString, @"^[A-Za-z0-9+/]*={0,2}$"))
            {
                try
                {
                    return Convert.FromBase64String(hashString);
                }
                catch
                {
                    // Se falhar, continua para próximo formato
                }
            }

            _logger.LogWarning("Hash format not recognized, using UTF8 encoding as fallback.");
            return System.Text.Encoding.UTF8.GetBytes(hashString);
        }

        public Task<byte[]> SignDocumentAsync(Stream documentStream, SignatureType signatureType, AlterCertificateInfo? certificateInfo, SignatureMetadata metadata, DocumentSignatureVisualContext visualContext, CancellationToken cancellationToken = default)
        {
            try
            {
                return signatureType switch
                {
                    SignatureType.Eletronic => SignEletronicallyAsync(documentStream, metadata, visualContext, cancellationToken),
                    SignatureType.DigitalA1 or SignatureType.DigitalA3 => SignWithCertficateAsync(documentStream, certificateInfo ?? throw new InvalidOperationException("Certificado digital é obrigatório para este tipo de assinatura."), metadata, visualContext, cancellationToken),
                    _ => throw new ArgumentException("Tipo de assinatura não suportado.", nameof(signatureType))
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error signing document with signature type {signatureType}");
                throw;
            }
        }

        public async Task<AlterCertificateInfo> ValidateCertificateAsync(byte[] certificateData, string? password = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var x509Certificate = X509CertificateLoader.LoadPkcs12(certificateData, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

                var validFrom = x509Certificate.NotBefore;
                var validTo = x509Certificate.NotAfter;
                var isValid = DateTime.UtcNow >= validFrom && DateTime.UtcNow <= validTo;

                var chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                var isChainValid = chain.Build(x509Certificate);

                if (!isChainValid)
                {
                    _logger.LogWarning("Certificate chain validation failed");
                    foreach (var status in chain.ChainStatus)
                    {
                        _logger.LogWarning("Chain status: {Status} - {StatusInformation}", 
                            status.Status, 
                            status.StatusInformation);
                    }
                }

                var certificateInfo = new AlterCertificateInfo(
                    serialNumber: x509Certificate.SerialNumber,
                    subjectName: x509Certificate.Subject,
                    issuerName: x509Certificate.Issuer,
                    validFrom: validFrom,
                    validTo: validTo,
                    thumbprint: x509Certificate.Thumbprint,
                    rawData: certificateData,
                    password: password
                );

                _logger.LogInformation(
                    "Certificate validated. Subject: {Subject}, Valid from: {ValidFrom}, Valid to: {ValidTo}, Is valid: {IsValid}",
                    certificateInfo.SubjectName,
                    certificateInfo.ValidFrom,
                    certificateInfo.ValidTo,
                    certificateInfo.isValid);
                
                return await Task.FromResult(certificateInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating certificate");
                throw new InvalidOperationException("Falha ao validar certificado digital. Verifique se o arquivo é um PKCS#12 válido e contém a chave privada.", ex);
            }
        }

        public Task<bool> ValidateSignatureAsync(Stream signedDocumentStream, CancellationToken cancellationToken = default)
        {
            try
            {
                using var pdfReader = new PdfReader(signedDocumentStream);
                using var pdfDocument = new PdfDocument(pdfReader);
                var signatureUtil = new SignatureUtil(pdfDocument);
                var signatureNames = signatureUtil.GetSignatureNames();

                if (!signatureNames.Any())
                {
                    _logger.LogWarning("No signatures found in the document.");
                    return Task.FromResult(false);
                }

                foreach (var signatureName in signatureNames)
                {
                    var pdfPKCS7 = signatureUtil.ReadSignatureData(signatureName);

                    if (pdfPKCS7 == null)
                    {
                        _logger.LogWarning("Signature {SignatureName} is not a valid PKCS#7 signature.", signatureName);
                        return Task.FromResult(false);
                    }

                    var signatureCoversWholeDocument = signatureUtil.SignatureCoversWholeDocument(signatureName);
                    if (!signatureCoversWholeDocument)
                    {
                        _logger.LogWarning("Signature {SignatureName} does not cover the whole document.", signatureName);
                        return Task.FromResult(false);
                    }

                    var isSignatureValid = pdfPKCS7.VerifySignatureIntegrityAndAuthenticity();
                    if (!isSignatureValid)
                    {
                        _logger.LogWarning("Signature {SignatureName} is invalid.", signatureName);
                        return Task.FromResult(false);
                    }
                }
                _logger.LogInformation("All signatures in the document are valid.");
                return Task.FromResult(true);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error validating signature.");
                throw;
            }
        }

        private async Task<byte[]> SignEletronicallyAsync(Stream documentStream, SignatureMetadata metadata, DocumentSignatureVisualContext visualContext, CancellationToken cancellationToken = default)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var pdfReader = new PdfReader(documentStream);
                // PDFs exportados com edicao restrita (sem senha de abertura) fazem o iText
                // recusar a escrita com BadPasswordException a menos que isto seja habilitado.
                pdfReader.SetUnethicalReading(true);
                using var pdfWriter = new PdfWriter(memoryStream);
                using var pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                ApplyVisualSignatureElements(pdfDocument, visualContext);

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

        private async Task<byte[]> SignWithCertficateAsync(Stream documentStream, AlterCertificateInfo certificateInfo, SignatureMetadata metadata, DocumentSignatureVisualContext visualContext, CancellationToken cancellationToken = default)
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
                var stamperProperties = new StampingProperties().UseAppendMode();

                var signerProperties = new SignerProperties();
                var fieldName = $"Signature_{DateTime.UtcNow:yyyyMMddHHmmss}";
                using var stagingStream = await PrepareVisualPdfForCertificateAsync(documentStream, visualContext, cancellationToken);
                using var pageCountReader = new PdfReader(new MemoryStream(stagingStream.ToArray()));
                pageCountReader.SetUnethicalReading(true);
                using var pageCountDocument = new PdfDocument(pageCountReader);
                var lastPageNumber = pageCountDocument.GetNumberOfPages();
                var preparedReader = new PdfReader(new MemoryStream(stagingStream.ToArray()));
                preparedReader.SetUnethicalReading(true);
                
                signerProperties
                    .SetFieldName(fieldName)
                    .SetReason("Assinatura Digital")
                    .SetLocation(metadata.Location ?? "Brasil")
                    .SetSignatureCreator("AGE SignatureHub");
            
                var appearance = new SignatureFieldAppearance(fieldName)
                    .SetContent($"Assinado digitalmente por {ResolveLatestSignerName(visualContext)}");
                
                signerProperties
                    .SetPageNumber(Math.Max(1, lastPageNumber))
                    .SetPageRect(new Rectangle(36, 160, 220, 90))
                    .SetSignatureAppearance(appearance);

                var signer = new PdfSigner(preparedReader, memoryStream, stamperProperties);
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

        private async Task<MemoryStream> PrepareVisualPdfForCertificateAsync(Stream originalStream, DocumentSignatureVisualContext visualContext, CancellationToken cancellationToken)
        {
            var preparedStream = new MemoryStream();
            if (originalStream.CanSeek)
            {
                originalStream.Position = 0;
            }

            await originalStream.CopyToAsync(preparedStream, cancellationToken);
            preparedStream.Position = 0;

            var output = new MemoryStream();
            using (var pdfReader = new PdfReader(preparedStream))
            {
                pdfReader.SetUnethicalReading(true);
                using var pdfWriter = new PdfWriter(output);
                using var pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                ApplyVisualSignatureElements(pdfDocument, visualContext);
            }

            output.Position = 0;
            return output;
        }

        private void ApplyVisualSignatureElements(PdfDocument pdfDocument, DocumentSignatureVisualContext visualContext)
        {
            if (pdfDocument.GetNumberOfPages() == 0)
            {
                pdfDocument.AddNewPage();
            }

            var footerText = $"Este documento foi assinado ({GetSignatureTypeLabel(visualContext.CurrentSignatureType)}) via Hub de Assinaturas nos termos da Lei 14.063/2020.";
            var verificationUrl = visualContext.VerificationUrl;
            var latestPage = pdfDocument.GetLastPage();

            DrawFooterOnPage(pdfDocument, latestPage, footerText, verificationUrl);
            DrawSignatureSummary(pdfDocument, latestPage, visualContext);
        }

        private void DrawFooterOnPage(PdfDocument pdfDocument, PdfPage page, string footerText, string verificationUrl)
        {
            var pageSize = page.GetPageSize();
            var footerHeight = 110f;
            var footerY = 18f;
            var footerWidth = pageSize.GetWidth() - 72f;
            var footerX = 36f;

            var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDocument);
            canvas.SaveState();
            canvas.SetFillColor(new DeviceRgb(250, 250, 252));
            canvas.Rectangle(footerX, footerY, footerWidth, footerHeight);
            canvas.Fill();
            canvas.RestoreState();

            var pageNumber = page.GetDocument().GetPageNumber(page);
            using var layoutCanvas = new LayoutCanvas(canvas, page.GetPageSize());
            var qrCode = new BarcodeQRCode(verificationUrl);
            var qrForm = qrCode.CreateFormXObject(ColorConstants.BLACK, pdfDocument);
            var qrImage = new Image(qrForm).ScaleAbsolute(72, 72);
            qrImage.SetFixedPosition(pageNumber, pageSize.GetWidth() - 120, footerY + 18);
            layoutCanvas.Add(qrImage);

            var paragraph = new Paragraph()
                .Add(new Text("Validação pública\n").SetFontSize(10))
                .Add(new Text($"{footerText}\n").SetFontSize(9))
                .Add(new Text(verificationUrl).SetFontSize(8).SetFontColor(ColorConstants.BLUE));

            paragraph.SetFixedPosition(pageNumber, footerX + 12, footerY + 18, footerWidth - 110);
            paragraph.SetFontSize(9);
            layoutCanvas.Add(paragraph);
        }

        private void DrawSignatureSummary(PdfDocument pdfDocument, PdfPage page, DocumentSignatureVisualContext visualContext)
        {
            var signedSigners = visualContext.SignedSigners
                .OrderBy(s => s.SignedAt)
                .GroupBy(s => s.SignerId)
                .Select(group => group.Last())
                .ToList();

            if (signedSigners.Count == 0)
            {
                return;
            }

            var pageNumber = page.GetDocument().GetPageNumber(page);
            var pageSize = page.GetPageSize();
            var startY = 135f;
            var leftX = 36f;
            var availableWidth = pageSize.GetWidth() - 72f;
            var columnCount = Math.Max(1, Math.Min(3, signedSigners.Count));
            var columnWidth = (availableWidth - ((columnCount - 1) * 18f)) / columnCount;
            using var layoutCanvas = new LayoutCanvas(new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDocument), page.GetPageSize());

            for (var i = 0; i < signedSigners.Count; i++)
            {
                var signer = signedSigners[i];
                var row = i / columnCount;
                var col = i % columnCount;
                var x = leftX + col * (columnWidth + 18f);
                var y = startY - row * 74f;

                var name = new Paragraph(signer.Name)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                    .SetFontSize(15)
                    .SetFontColor(new DeviceRgb(31, 41, 55));
                name.SetFixedPosition(pageNumber, x, y + 28f, columnWidth);
                layoutCanvas.Add(name);

                var lineCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDocument);
                lineCanvas.SetLineWidth(0.75f);
                lineCanvas.SetStrokeColor(new DeviceRgb(148, 163, 184));
                lineCanvas.MoveTo(x, y + 24f);
                lineCanvas.LineTo(x + columnWidth, y + 24f);
                lineCanvas.Stroke();

                var meta = new Paragraph($"{GetSignatureTypeLabel(signer.SignatureType)} • {signer.SignedAt:dd/MM/yyyy HH:mm}")
                    .SetFontSize(8)
                    .SetFontColor(new DeviceRgb(71, 85, 105));
                meta.SetFixedPosition(pageNumber, x, y + 8f, columnWidth);
                layoutCanvas.Add(meta);
            }
        }

        private static string ResolveLatestSignerName(DocumentSignatureVisualContext visualContext)
        {
            return visualContext.SignedSigners
                .OrderByDescending(s => s.SignedAt)
                .Select(s => s.Name)
                .FirstOrDefault() ?? "AGE SignatureHub";
        }

        private static string GetSignatureTypeLabel(SignatureType signatureType) => signatureType switch
        {
            SignatureType.Eletronic => "Assinatura Eletrônica",
            SignatureType.DigitalA1 => "Certificado Digital A1",
            SignatureType.DigitalA3 => "Certificado Digital A3",
            SignatureType.Biometric => "Biometria",
            _ => signatureType.ToString()
        };
    }
}
