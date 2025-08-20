using Microsoft.AspNetCore.Mvc;
using InfoPointServer.Services;

namespace InfoPointServer.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // 🔍 Endpoint clasic — fără paginare
    [HttpGet]
    public IActionResult Get(
        [FromQuery] string term,
        [FromQuery] string? category)
    {
        var requestTime = DateTime.UtcNow;
        var tabletId = Request.Headers["Tablet-ID"].FirstOrDefault() ?? "UNKNOWN";

        _logger.LogInformation($"📡 Tablet {tabletId} searched for '{term}', category '{category}' at {requestTime}");

        var results = _service.Search(term, category);

        _logger.LogInformation($"✅ Returned {results.Count} items in {DateTime.UtcNow.Subtract(requestTime).TotalMilliseconds:N0} ms");

        return Ok(results);
    }

    // 📦 Nou: Endpoint cu paginare
    [HttpGet("paged")]
    public IActionResult GetPaged(
        [FromQuery] string term,
        [FromQuery] string? category,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 40)
    {
        var requestTime = DateTime.UtcNow;
        var tabletId = Request.Headers["Tablet-ID"].FirstOrDefault() ?? "UNKNOWN";

        _logger.LogInformation($"📃 Tablet {tabletId} requested page {page} of term '{term}', category '{category}' at {requestTime}");

        var pagedResults = _service.Search(term, category, page, pageSize);

        _logger.LogInformation($"🧮 Returned {pagedResults.Items.Count} items from page {page} in {DateTime.UtcNow.Subtract(requestTime).TotalMilliseconds:N0} ms");

        return Ok(pagedResults);
    }
}
