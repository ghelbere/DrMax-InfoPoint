using InfoPoint.Models;

namespace InfoPointServer.Services;

public interface IProductService
{
    List<ProductDto> Search(string term, string? category);
    PagedProductResult<ProductDto> Search(string term, string? category, int page, int pageSize);
}

public class DummyProductService : IProductService
{
    public List<ProductDto> Search(string term, string? category)
    {
        return new(); // Search(term, category, page: 0, pageSize: 10000); // fallback la tot
    }

    public PagedProductResult<ProductDto> Search(string term, string? category, int page, int pageSize)
    {
        var allProducts = new List<ProductDto>();
        for (var i = 0; i < 10000; i++)
        {
            allProducts.Add(new()
            {
                Id = 1 + i,
                Name = $"Aspirin {i}",
                Price = 8.99m,
                Location = $"Raft {1 + i}",
                ImageUrl = "",
                Category = "Cosmetice"
            });
            allProducts.Add(new()
            {
                Id = 2 + i,
                Name = $"Paracetamol {i}",
                Price = 9.50m,
                Location = $"Zona rece {1 + i}",
                ImageUrl = "https://images.rawpixel.com/image_png_social_landscape/cHJpdmF0ZS9sci9pbWFnZXMvd2Vic2l0ZS8yMDIzLTExL3Jhd3BpeGVsb2ZmaWNlMV9hX3NpbXBsZV95ZXRfcmVhbGlzdGljX3Bob3RvX29mX2FfY29sbGVjdGlvbl9vZl8yN2JkMzE4OS1hZTI4LTQyMGItOTkwYy1hN2ViZDMwMDZjZjIucG5n.png",
                Category = "Durere"
            });
            allProducts.Add(new()
            {
                Id = 3 + i,
                Name = $"Nurofen 400 {i}",
                Price = 12.99m,
                Location = $"Raft {1 + i}",
                ImageUrl = @"Nurofen400.png",
                Category = "Durere"
            });
            allProducts.Add(new()
            {
                Id = 4 + i,
                Name = $"Nurofen Forte 400 {i} Cinque",
                Price = 27.99m,
                OriginalPrice = 38.99m,
                Location = $"Raft {1 + i}",
                ImageUrl = @"nurofen_forte_400.png",
                Category = "Durere"
            });
            allProducts.Add(new()
            {
                Id = 5 + i,
                Name = $"Orteză genunchi {i}",
                Price = 42.99m,
                Location = "Raftul 7 de la geam",
                ImageUrl = "https://www.orteze.ro/client/uploads.images/1541772479_-_1.medium.jpg",
                Category = "Proteze"
            });
        }

        var filtered = allProducts
            .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                     && (string.IsNullOrWhiteSpace(category) || p.Category == category))
            .ToList();

        var pagedItems = filtered
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedProductResult<ProductDto>
        {
            Items = pagedItems,
            TotalItems = filtered.Count,
            PageSize = pageSize
        };
    }
}
