using InfoPoint.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace InfoPoint.Models
{
    public partial class ProductDto : INotifyPropertyChanged
    {
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

    }
}
