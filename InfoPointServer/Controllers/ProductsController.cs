using InfoPointServer.Models;
using InfoPointServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfoPointServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductSearchService _productSearchService;
        private readonly IConfiguration _config;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductSearchService productSearchService, IConfiguration config, ILogger<ProductsController> logger)
        {
            _productSearchService = productSearchService;
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts(
            CancellationToken cancellationToken,
            [FromQuery] string query,
            [FromQuery] string? category,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 15
            )
        {
            var requestTime = DateTime.UtcNow;
            var tabletId = Request.Headers["Tablet-ID"].FirstOrDefault() ?? "UNKNOWN";

            _logger.LogInformation($"📃 Tablet {tabletId} requested page {page} of term '{query}', category '{category}' at {requestTime}");

            var storeId = _config["Store:Id"]; // ← din config

            List<ProductDto>? products = null;
            try
            {
                products = await _productSearchService.SearchProductsAsync(query, category, storeId ?? "UNKNOWN", cancellationToken);

                _logger.LogInformation($"🧮 Returned {products.Count} items from page {page} in {DateTime.UtcNow.Subtract(requestTime).TotalMilliseconds:N0} ms");
            }
            catch (Exception ex) {
                return BadRequest(new { error = ex.Message });
            }

            return Ok(products);
        }
    }
}
