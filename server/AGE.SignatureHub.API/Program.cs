using AGE.SignatureHub.Application.Configuration;
using Serilog;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using System.Text;
using AGE.SignatureHub.Application;
using AGE.SignatureHub.Infrastructure;
using Microsoft.OpenApi;
using AGE.SignatureHub.Infrastructure.Persistence;
using AGE.SignatureHub.API.Middleware;
using AGE.SignatureHub.Application.BackgroundJobs;
using Hangfire.Dashboard;
using AGE.SignatureHub.Infrastructure.Persistence.Seed;

try
{
    
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

    builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
    );

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "AGE SignatureHub API",
            Version = "v1",
            Description = "API for managing digital signatures and document workflows.",
            Contact = new OpenApiContact
            {
                Name = "AGE Support",
                Email = "desenvolvimento@advocaciageral.mg.gov.br"
            }
        });
    });

    builder.Services.AddApplication();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection(ApplicationSettings.SectionName));

    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        builder.Services.AddSwaggerGen(c => c.IncludeXmlComments(xmlPath));
    }

    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(
                c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AGE SignatureHub API V1");
                    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
                }
        );
    }

    app.UseCors("AllowAll");

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDBContext>();
        var seeder = services.GetRequiredService<DatabaseSeeder>();

        try
        {
            Log.Information("Applying database migrations...");
            dbContext.Database.Migrate();
            Log.Information("Database migrations applied successfully.");

            Log.Information("Seeding database...");
            await seeder.SeedAsync();
            Log.Information("Database seeding completed successfully.");
        }
        catch (System.Exception ex)
        {
            Log.Error(ex, "An error occurred while applying database migrations.");
        }
    }

    ConfigureBackgroundJobs(app.Services);

    Log.Information("Starting AGE SignatureHub API...");

    var urls = app.Urls.ToList();
    if (urls.Count == 0)
    {
        urls.Add("http://localhost:5000");
        urls.Add("https://localhost:5001");
    }

    var primaryUrl = urls[0];
    Log.Information("╔════════════════════════════════════════════════════════════╗");
    Log.Information("║          AGE SignatureHub API is running at:               ║");
    Log.Information("╠════════════════════════════════════════════════════════════╣");

    foreach (var url in urls)
    {
        Log.Information("║  {Url,-55} ║", url);
    }

    Log.Information("╠════════════════════════════════════════════════════════════╣");
    Log.Information("║  Swagger UI: {Url,-44}║", $"{primaryUrl}/swagger");
    Log.Information("║  Health Check: {Url,-42}║", $"{primaryUrl}/health");
    Log.Information("║  Hangfire: {Url,-46}║", $"{primaryUrl}/hangfire");
    Log.Information("╚════════════════════════════════════════════════════════════╝");

    await app.RunAsync();
}
catch (System.Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

void ConfigureBackgroundJobs(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var backgroundJobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobsService>();
    backgroundJobService.CheckeExpiredDocuments();

    backgroundJobService.CleanupOldAuditLogs(365);

    Log.Information("Background jobs configured successfully.");
}

public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // Implement your authorization logic here
        // For example, you can check if the user is authenticated and has a specific role
        var httpContext = context.GetHttpContext();
        return httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() || (httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin"));
    }
}

