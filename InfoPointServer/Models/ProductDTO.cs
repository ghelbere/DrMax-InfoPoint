namespace InfoPointServer.Models
{
    public class ProductDto
    {
        public int Id { get; set; }                     // din MSSQL
        public string Sku { get; set; } = string.Empty; // din MSSQL
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string Location { get; set; } = string.Empty; // din Quant
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;    // din Quant
        public string PositionHint { get; set; } = string.Empty; // din Quant
        public string PlanogramImageBase64 { get; set; } = string.Empty; // din Quant
    }
}
