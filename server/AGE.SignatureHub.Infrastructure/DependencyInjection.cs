using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.BackgroundJobs;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Infrastructure.Configuration;
using AGE.SignatureHub.Infrastructure.Persistence;
using AGE.SignatureHub.Infrastructure.Services.BackgroundJobs;
using AGE.SignatureHub.Infrastructure.Services.Cryptography;
using AGE.SignatureHub.Infrastructure.Services.Email;
using AGE.SignatureHub.Infrastructure.Services.Signature;
using AGE.SignatureHub.Infrastructure.Services.Storage;
using AGE.SignatureHub.Infrastructure.Services.Timestamp;
using AGE.SignatureHub.Infrastructure.Services.Webhook;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AGE.SignatureHub.Infrastructure
{
    public static class DependencyInjection
    {
        [Obsolete]
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDBContext>(options => 
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDBContext).Assembly.FullName)
                )
            );
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<StorageSettings>(configuration.GetSection("Storage"));
            services.Configure<EmailSettings>(configuration.GetSection("Email"));
            services.Configure<WebhookSettings>(configuration.GetSection("Webhooks"));

            var storageProvider = configuration["Storage:Provider"];

            if (string.IsNullOrEmpty(storageProvider))
            {
                throw new Exception("Storage provider is not configured. Please set 'Storage:Provider' in the configuration.");
            }

            if (storageProvider == "AzureBlob")
            {
                services.AddScoped<IStorageService, AzureBlobStorageService>();
            }
            else if (storageProvider == "LocalFileSystem")
            {
                services.AddScoped<IStorageService, LocalFileStorageService>();
            }
            else
            {
                throw new Exception($"Unsupported storage provider: {storageProvider}");
            }

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICryptographyService, CryptographyService>();
            services.AddScoped<ISignatureService, SignatureService>();
            services.AddScoped<IWebhookService, WebhookService>();
            services.AddScoped<IBackgroundJobsService, BackgroundJobService>();

            services.AddHttpClient<ITimestampService, TimestampService>();
            services.AddHttpClient<IWebhookService, WebhookService>();

            services.AddHangfire(config => 
                config.UsePostgreSqlStorage(configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddHangfireServer();

            return services;
        }
    }
}