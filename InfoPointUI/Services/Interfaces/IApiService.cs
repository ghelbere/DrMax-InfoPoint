using InfoPoint.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfoPointUI.Services.Interfaces
{
    public interface IApiService
    {
        Task<PagedProductResult<ProductDto>> SearchProductsPagedAsync(
            string query,
            string tabletId,
            string? category,
            int page,
            int pageSize);
    }

}