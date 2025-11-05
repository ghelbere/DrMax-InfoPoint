using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InfoPointUI.Commands;
using InfoPointUI.Models;
using InfoPointUI.Services;

namespace InfoPointUI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ProductDto> Products { get; set; } = new();

    private string _searchTerm = "";
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm != value)
            {
                _searchTerm = value;
                CurrentPage = 0;
                OnPropertyChanged();
                debounceTimer.Stop();
                debounceTimer.Start();
                ((RelayCommand<object>)SearchCommand).RaiseCanExecuteChanged();
            }
        }
    }

    private string? _selectedCategory = "Toate";
    public string? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                CurrentPage = 0;
                OnPropertyChanged();
                _ = SearchAsync();
            }
        }
    }

    // 🔢 Paginare
    public int PageSize { get; set; } = 28;

    private int _currentPage = 0;
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged();
                _ = SearchAsync();
                OnPropertyChanged(nameof(DisplayPage));
                OnPropertyChanged(nameof(IsLastPage));
            }
        }
    }

    public int DisplayPage => (TotalPages<1)?0:CurrentPage + 1;

    private int _totalPages = 1;
    public int TotalPages
    {
        get => _totalPages;
        set
        {
            if (_totalPages != value)
            {
                _totalPages = value <= 0 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLastPage)); // ne asigurăm că se reevaluează
            }
        }
    }


    public bool IsLastPage => DisplayPage >= TotalPages;

    public ICommand SearchCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    private readonly System.Timers.Timer debounceTimer;
    private readonly ApiService _api = new();
    private readonly string TabletId = "TAB-999";

    public MainViewModel()
    {
        TotalPages = 1;         // sau 0, dar ideal e să fie > 0

        // Debounce timer for searching
        debounceTimer = new(500);
        debounceTimer.Elapsed += async (_, _) => await SearchAsync();
        debounceTimer.AutoReset = false;

        SearchCommand = new RelayCommand<object>(
            async _ => await SearchAsync(),
            _ => !string.IsNullOrWhiteSpace(SearchTerm)
        );

        SelectCategoryCommand = new RelayCommand<string>(category =>
        {
            SelectedCategory = category;
        });

        NextPageCommand = new RelayCommand<object>(_ =>
        {
            if (Products.Count < PageSize)
            {
                return;
            }

            CurrentPage++;
        });

        PreviousPageCommand = new RelayCommand<object>(_ =>
        {
            if (CurrentPage > 0)
                CurrentPage--;
        });
    }

    public async Task SearchAsync()
    {
        var result = await _api.SearchProductsPagedAsync(
            SearchTerm,
            TabletId,
            SelectedCategory == "Toate" ? null : SelectedCategory,
            CurrentPage,
            PageSize);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Products.Clear();
            foreach (var product in result.Items)
            {
                Products.Add(product);
                _ = LoadProductImageAsync(product); // fire-and-forget
            }

            TotalPages = result.TotalPages;
        });
    }

    private async Task LoadProductImageAsync(ProductDto product)
    {
        ImageSource? image = null;

        // 1️⃣ Verifică dacă imaginea există ca StaticResource în App.xaml
        if (Application.Current.Resources.Contains(product.ImageUrl.ToLower()))
            try 
            {
                var uriString = Application.Current.Resources[product.ImageUrl.ToLower()] as string;
                if (!string.IsNullOrEmpty(uriString))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(uriString, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    image = bmp;
                }
            } catch {
                image = null;
            }

        // 2️⃣ Dacă nu e resursă, încearcă să o încarce din URL
        if (image == null && Uri.TryCreate(product.ImageUrl, UriKind.Absolute, out Uri uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            try
            {
                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync(uri);
                using var ms = new MemoryStream(bytes);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                image = bmp;
            }
            catch
            {
                // Ignorăm excepția, trecem la fallback
            }
        }

        // 3️⃣ Fallback: imagine default
        if (image == null)
        {
            image = new BitmapImage(new Uri("pack://application:,,,/InfoPointUI;component/Assets/Images/empty_image.png"));
        }

        product.ProductImage = image;
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new(name));
}
