
using MimeKit;
using AGE.SignatureHub.Application.Configuration;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;

namespace AGE.SignatureHub.Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }
        public Task SendDocumentExpiredAsync(string toEmail, string toName, string documentTitle, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SendReminderAsync(string toEmail, string toName, string documentTitle, string signatureUrl, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task SendSignatureCompletedAsync(string toEmail, string toName, string documentTitle, CancellationToken cancellationToken = default)
        {
            var subject = $"Documento '{documentTitle}' Assinado com Sucesso";
            var body = GetSignatureCompletedTemplate(toName, documentTitle);

            await SendEmailAsync(toEmail, toName, subject, body, cancellationToken);
        }

        public async Task SendSignatureRejectedAsync(string toEmail, string toName, string documentTitle, string reason, CancellationToken cancellationToken = default)
        {
            var subject = $"Documento '{documentTitle}' Rejeitado";
            var body = GetSignatureRejectedTemplate(toName, documentTitle, reason);

            await SendEmailAsync(toEmail, toName, subject, body, cancellationToken);
        }

        public async Task SendSignatureRequestAsync(string toEmail, string toName, string documentTitle, string signatureUrl, CancellationToken cancellationToken = default)
        {
            var subject = $"Solicitação de assinatura para '{documentTitle}'";
            var body = GetSignatureRequestTemplate(toName, documentTitle, signatureUrl);

            await SendEmailAsync(toEmail, toName, subject, body, cancellationToken);
        }

        private async Task SendEmailAsync(string toEmail, string toName, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _settings.SmtpServer,
                    _settings.SmtpPort,
                    _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls,
                    cancellationToken
                );

                await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'", toEmail, subject);
                throw;
            }
        }

        private string GetSignatureRequestTemplate(string toName, string documentTitle, string signatureUrl)
        {
            return 
            $@"
            <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #0066cc; color: white; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f9f9f9; padding: 30px; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #0066cc; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>SignatureHub AGE</h1>
                        </div>
                        <div class='content'>
                            <h2>Olá, {toName}!</h2>
                            <p>Você foi solicitado(a) a assinar o seguinte documento:</p>
                            <p><strong>{documentTitle}</strong></p>
                            <p>Para visualizar e assinar o documento, clique no botão abaixo:</p>
                            <a href='{signatureUrl}' class='button'>Assinar Documento</a>
                            <p style='margin-top: 30px; font-size: 14px; color: #666;'>
                                Ou copie e cole o seguinte link no seu navegador:<br>
                                <a href='{signatureUrl}'>{signatureUrl}</a>
                            </p>
                        </div>
                        <div class='footer'>
                            <p>Este é um e-mail automático. Por favor, não responda.</p>
                            <p>&copy; {DateTime.Now.Year} AGE - Advocacia-Geral do Estado</p>
                        </div>
                    </div>
                </body>
            </html>";
        }

        private string GetSignatureCompletedTemplate(string name, string documentTitle)
        {
            return 
            $@"
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f9f9f9; padding: 30px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>✓ Documento Assinado</h1>
                        </div>
                        <div class='content'>
                            <h2>Olá, {name}!</h2>
                            <p>O documento <strong>{documentTitle}</strong> foi assinado com sucesso por todos os signatários.</p>
                            <p>O processo de assinatura está completo.</p>
                        </div>
                        <div class='footer'>
                            <p>Este é um e-mail automático. Por favor, não responda.</p>
                            <p>&copy; {DateTime.Now.Year} AGE - Advocacia-Geral do Estado</p>
                        </div>
                    </div>
                </body>
            </html>";
        }

        private string GetSignatureRejectedTemplate(string name, string documentTitle, string reason)
        {
            return $@"
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f9f9f9; padding: 30px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>✗ Documento Rejeitado</h1>
                        </div>
                        <div class='content'>
                            <h2>Olá, {name}!</h2>
                            <p>O documento <strong>{documentTitle}</strong> foi rejeitado.</p>
                            <p><strong>Motivo:</strong> {reason}</p>
                        </div>
                        <div class='footer'>
                            <p>Este é um e-mail automático. Por favor, não responda.</p>
                            <p>&copy; {DateTime.Now.Year} AGE - Advocacia-Geral do Estado</p>
                        </div>
                    </div>
                </body>
            </html>";
        }

        private string GetDocumentExpiredTemplate(string name, string documentTitle)
        {
            return $@"
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #ffc107; color: #333; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f9f9f9; padding: 30px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>⏱ Documento Expirado</h1>
                        </div>
                        <div class='content'>
                            <h2>Olá, {name}!</h2>
                            <p>O prazo para assinar o documento <strong>{documentTitle}</strong> expirou.</p>
                            <p>O documento não está mais disponível para assinatura.</p>
                        </div>
                        <div class='footer'>
                            <p>Este é um e-mail automático. Por favor, não responda.</p>
                            <p>&copy; {DateTime.Now.Year} AGE - Advocacia-Geral do Estado</p>
                        </div>
                    </div>
                </body>
            </html>";
        }

        private string GetReminderTemplate(string name, string documentTitle, string signatureUrl)
        {
            return $@"
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #ff9800; color: white; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f9f9f9; padding: 30px; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #ff9800; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔔 Lembrete de Assinatura</h1>
                        </div>
                        <div class='content'>
                            <h2>Olá, {name}!</h2>
                            <p>Este é um lembrete de que você ainda tem um documento pendente de assinatura:</p>
                            <p><strong>{documentTitle}</strong></p>
                            <p>Para visualizar e assinar o documento, clique no botão abaixo:</p>
                            <a href='{signatureUrl}' class='button'>Assinar Documento</a>
                        </div>
                        <div class='footer'>
                            <p>Este é um e-mail automático. Por favor, não responda.</p>
                            <p>&copy; {DateTime.Now.Year} AGE - Advocacia-Geral do Estado</p>
                        </div>
                    </div>
                </body>
            </html>";
        }
    }
}