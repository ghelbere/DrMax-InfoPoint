using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace InfoPointUI.Models
{
    public class ProductDto : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty; // din MSSQL
        public string Name { get; set; } = "";

        public string Description { get; set; } = "Test product Description";
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string Category { get; set; } = "Durere";
        public string Location { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Block { get; set; } = string.Empty;    // din Quant
        public string PositionHint { get; set; } = string.Empty; // din Quant
        public string PlanogramImageBase64 { get; set; } = string.Empty; // din Quant

        public bool HasDiscount => OriginalPrice.HasValue && OriginalPrice > Price;
        public int DiscountPercentage =>
            HasDiscount ? (int)Math.Round((OriginalPrice!.Value - Price) / OriginalPrice.Value * 100) : 0;
        public string DiscountLabel => HasDiscount ? $"-{DiscountPercentage}%" : string.Empty;

        private ImageSource? _productImage;
        public ImageSource? ProductImage
        {
            get => _productImage;
            set
            {
                _productImage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new(name));
    }
}
