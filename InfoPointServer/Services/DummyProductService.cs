using InfoPoint.Models;
using InfoPointServer.Interfaces;

namespace InfoPointServer.Services;

public class DummyProductService : IProductService
{
    public PagedProductResult<ProductDto> Search(string term, string? category, int page, int pageSize)
    {
        var allProducts = new List<ProductDto>();
        for (var i = 0; i < 10000; i++)
        {
            allProducts.Add(new()
            {
                Id = 1 + i,
                Name = $"Cremă hidratantă cu apă termală SPF 20 Eau Thermale, 40ml, Uriage {i}",
                Price = 8.99m,
                Location = $"Raft {1 + i}",
                ImageUrl = "https://www.drmax.ro/_i/215245752.webp?m2=%2Fmedia%2Fcatalog%2Fproduct%2F3%2F6%2F3661434005039_eau_thermale_crema_hidratanta_cu_spf20_40ml_rezultate.png&fit=contain&w=350&h=350&format=webp",
                Category = "Cosmetice"
            });
            allProducts.Add(new()
            {
                Id = 1 + i,
                Name = $"Șampon tratament antimatreață cu apă termală DS Hair, 200ml, Uriage {i}",
                Price = 8.99m,
                Location = $"Raft {1 + i}",
                ImageUrl = "https://www.drmax.ro/_i/1980779185.webp?m2=%2Fmedia%2Fcatalog%2Fproduct%2Fd%2Fs%2Fds_hair_sampon_tratament_uriage.jpg&fit=contain&w=350&h=350&format=webp",
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
                Category = "Proteze",
                IsInStock = i % 3 == 0
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

    public List<ProductDto> Search(string term, string? category)
    {
        return new List<ProductDto>();
    }
}
