using InfoPoint.Models;
using Microsoft.Data.SqlClient;

namespace InfoPointServer.Services
{
    public class ProductSearchService
    {
        private readonly IConfiguration _config;
        private readonly QuantPlacementService _quantService;

        public ProductSearchService(IConfiguration config, QuantPlacementService quantService)
        {
            _config = config;
            _quantService = quantService;
        }

        public async Task<List<ProductDto>> SearchProductsAsync(string query, string? category, string storeId, CancellationToken cancellationToken)
        {
            var productsFromDb = await SearchInDatabaseAsync(query, category, cancellationToken);
            var skus = productsFromDb.Select(p => p.Sku).ToList();

            var placements = await _quantService.GetPlacementsAsync(skus, storeId, cancellationToken);

            var result = new List<ProductDto>();
            foreach (var dbProduct in productsFromDb)
            {
                var match = placements.FirstOrDefault(p => p.ProductCode == dbProduct.Sku);
                result.Add(new ProductDto
                {
                    Id = dbProduct.Id,
                    Sku = dbProduct.Sku,
                    Name = dbProduct.Name,
                    Price = dbProduct.Price,
                    OriginalPrice = dbProduct.OriginalPrice,
                    ImageUrl = dbProduct.ImageUrl,
                    Category = dbProduct.Category,
                    Location = match?.Fixture?.Name ?? "",
                    Block = match?.Fixture?.Block ?? "",
                    PositionHint = match?.Fixture?.PositionHint ?? "",
                    PlanogramImageBase64 = match?.PlanogramImageBase64 ?? ""
                });
            }

            return result;
        }

        private async Task<List<ProductDto>> SearchInDatabaseAsync(string query, string? category, CancellationToken cancellationToken)
        {
            var result = new List<ProductDto>();
            var connectionString = _config.GetConnectionString("ProductDb");

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = new SqlCommand(@"
                SELECT Id, Sku, Name, Price, OriginalPrice, ImageUrl, Category
                FROM Products
                WHERE Name LIKE @query OR Category LIKE @query", conn);
            cmd.Parameters.AddWithValue("@query", $"%{query}%");

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new ProductDto
                {
                    Id = reader.GetInt32(0),
                    Sku = reader.GetString(1),
                    Name = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    OriginalPrice = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    ImageUrl = reader.GetString(5),
                    Category = reader.GetString(6)
                });
            }

            return result;
        }
    }
}
