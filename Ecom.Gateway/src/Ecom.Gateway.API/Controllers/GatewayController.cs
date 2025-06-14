[ApiController]
[Route("api/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(ILogger<GatewayController> logger)
    {
        _logger = logger;
    }

    [HttpGet("info")]
    public IActionResult GetGatewayInfo()
    {
        var info = new
        {
            Service = "Ecommerce Gateway",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Timestamp = DateTime.UtcNow,
            Routes = new[]
            {
                new { Path = "/api/core/**", Target = "Core API Service" },
                new { Path = "/api/users/**", Target = "Users API Service" }
            }
        };

        _logger.LogInformation("Gateway info requested");
        return Ok(info);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
