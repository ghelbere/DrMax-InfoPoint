namespace InfoPointUI.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Category { get; set; } = "Durere";
    public string Location { get; set; } = "";
    public string ImageUrl { get; set; } = "";

    public bool HasDiscount => OriginalPrice.HasValue && OriginalPrice > Price;
    public int DiscountPercentage =>
        HasDiscount ? (int)Math.Round((OriginalPrice.Value - Price) / OriginalPrice.Value * 100) : 0;

    public string DiscountLabel => HasDiscount ? $"-{DiscountPercentage}% promo" : string.Empty;
}
