using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDBContext dbContext, ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Get()
        {
            try
            {
                await _dbContext.Database.CanConnectAsync();

                return Ok(new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Service = "AGE SignatureHub API"
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Service = "AGE SignatureHub API",
                    Error = ex.Message
                });
            }
        }

    }
}