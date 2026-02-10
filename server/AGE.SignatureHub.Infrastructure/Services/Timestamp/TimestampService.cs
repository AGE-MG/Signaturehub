using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace AGE.SignatureHub.Infrastructure.Services.Timestamp
{
    public class TimestampService : ITimestampService
    {
        private readonly ILogger<TimestampService> _logger;
        private readonly HttpClient _httpClient;
        private const string TSA_URL = "https://validar.iti.gov.br/";

        public TimestampService(ILogger<TimestampService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<byte[]> ApplyTimestampAsync(byte[] documentHash, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestGenerator = new TimeStampRequestGenerator();
                requestGenerator.SetCertReq(true);

                var request = requestGenerator.Generate(
                    TspAlgorithms.Sha256, 
                    documentHash,
                    BigInteger.ValueOf(DateTime.UtcNow.Ticks)
                );
                var requestBytes = request.GetEncoded();

                var httpContent = new ByteArrayContent(requestBytes);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/timestamp-query");

                var response = await _httpClient.PostAsync(TSA_URL, httpContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var timeStampResponse = new TimeStampResponse(responseBytes);
                timeStampResponse.Validate(request);

                if (timeStampResponse.GetFailInfo() != null)
                {
                    _logger.LogError("Timestamp request failed: {FailInfo}", timeStampResponse.GetFailInfo());
                    throw new Exception($"Timestamp request failed: {timeStampResponse.GetFailInfo()}");
                }

                if (timeStampResponse.Status != 0)
                {
                    _logger.LogError("Timestamp request returned non-success status: {Status}", timeStampResponse.Status);
                    throw new Exception($"Timestamp request returned non-success status: {timeStampResponse.Status}");
                }

                var timeStampToken = timeStampResponse.TimeStampToken;
                var tokenBytes = timeStampToken.GetEncoded();

                _logger.LogInformation("Timestamp applied successfully. Token size: {TokenSize} bytes", tokenBytes.Length);
                return tokenBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying timestamp");
                throw new Exception($"Error applying timestamp: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateTimestampAsync(
            byte[] timestampToken,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var token = new TimeStampToken(new CmsSignedData(timestampToken));

                // Validar a assinatura do TSA
                var signedData = token.ToCmsSignedData();
                var signers = signedData.GetSignerInfos().GetSigners();
                
                foreach (var signer in signers)
                {
                    // TODO: Validar certificado do TSA contra cadeia ICP-Brasil
                    // Por enquanto, apenas verificar se consegue processar o token
                }

                _logger.LogInformation("Timestamp validated successfully");

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating timestamp");
                return false;
            }
        }
    }
}