using InfoPoint.Models;

namespace InfoPointServer.Interfaces
{
    public interface IProductService
    {
        List<ProductDto> Search(string term, string? category);
        PagedProductResult<ProductDto> Search(string term, string? category, int page, int pageSize);
    }
}
