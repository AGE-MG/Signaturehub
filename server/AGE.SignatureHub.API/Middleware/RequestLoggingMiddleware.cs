using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            _logger.LogInformation("Handling {Method} request for {Path}", requestMethod, requestPath);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation("Finished handling {Method} request for {Path} in {ElapsedMilliseconds} ms",
                    requestMethod, requestPath, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}