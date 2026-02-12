using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Configuration;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AGE.SignatureHub.Infrastructure.Services.Webhook
{
    public class WebhookService : IWebhookService
    {
        private readonly WebhookSettings _settings;
        private readonly ILogger<WebhookService> _logger;
        private readonly HttpClient _httpClient;

        public WebhookService(IOptions<WebhookSettings> settings, ILogger<WebhookService> logger, HttpClient httpClient)
        {
            _settings = settings.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task SendWebhookAsync(string eventType, string payload, CancellationToken cancellationToken = default)
        {
            try
            {
                var relevantEndpoints = _settings.Endpoints.Where(e => e.Events.Contains(eventType) || e.Events.Contains("*")).ToList();
                if (!relevantEndpoints.Any())
                {
                    _logger.LogInformation("No webhook endpoints configured for event type {EventType}", eventType);
                    return;
                }

                var webhookPayload = new
                {
                    EventType = eventType,
                    Data = payload,
                    Timestamp = DateTime.UtcNow
                };

                var jsonPayload = JsonSerializer.Serialize(webhookPayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                var tasks = relevantEndpoints.Select(endpoint => SendToEndpointAsync(endpoint, jsonPayload, cancellationToken));

                await Task.WhenAll(tasks);
                _logger.LogInformation("Completed sending webhooks for event type {EventType}", eventType);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for event type {EventType}", eventType);
                
            }
        }

        private async Task SendToEndpointAsync(Application.Configuration.WebhookEndpoint endpoint, string jsonPayload, CancellationToken cancellationToken)
        {
            try
            {
                
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrEmpty(endpoint.Secret))
                {
                    var signature = GenerateHmacSignature(jsonPayload, endpoint.Secret);
                    request.Headers.Add("X-webhook-Signature", signature);
                }

                request.Headers.Add("User-Agent", "AGE.SignatureHub.WebhookClient/1.0");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent webhook to {EndpointName} ({Url})", endpoint.Name, endpoint.Url);
                } else
                {
                    _logger.LogError("Failed to send webhook to {EndpointName} ({Url}). Status Code: {StatusCode}", endpoint.Name, endpoint.Url, response.StatusCode);
                }

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook to {Url}", endpoint.Url);
            }
        }


        private string GenerateHmacSignature(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }
    }
}